// Clase principal del juego
class BuzoGame {
    constructor(containerId) {
        // Configuración básica de Phaser
        this.config = {
            width: 1280,
            height: 960,
            parent: containerId,
            type: Phaser.AUTO,
            backgroundColor: 'rgba(0,0,0,0)', // Fondo transparente
            scene: {
                preload: this.preload.bind(this),
                create: this.create.bind(this),
                update: this.update.bind(this)
            },
            physics: {
                default: "arcade",
            },
            scale: {
                mode: Phaser.Scale.FIT,
                autoCenter: Phaser.Scale.CENTER_BOTH
            },
            render: {
                pixelArt: false,
                antialias: true,
                autoResize: true
            },
            transparent: true
        };

        // Variables globales
        this.cursors = null;
        this.spaceKey = null;
        this.buzo = null;
        this.burbuja = null;
        this.isInhaling = false;
        this.basePosition = 150; // Posición base del buzo
        this.bubbleOffset = 300;   // Desplazamiento por respiración

        // Variables para los obstáculos
        this.obstacles = [];
        this.obstacleSpeed = 200; // Velocidad en píxeles por segundo
        this.nextIsCoral = true;  // Alternamos coral y tiburón

        // Variables para el ciclo de respiración
        this.respiracionBarraProgreso = 0;
        this.barraLlenando = true;  // true = inhalar, false = exhalar
        this.barraGrafica = null;          // Gráfico de la barra de progreso
        this.barraFondo = null;            // Fondo de la barra
        this.textoRespiracion = null;      // Texto que indica "Inhala" o "Exhala"
        this.iconoRespiracion = null;      // Icono de flecha

        // Variables de estado del juego
        this.hasStarted = false;
        this.countdown = null;
        this.countdownText = null;
        this.music = null;

        // Variables de tiempo
        this.lastObstacleTime = 0;
        this.respiracionTimer = 0;
        this.cycleCompleted = false;

        // Iniciar el juego
        this.game = new Phaser.Game(this.config);
    }

    preload() {
        // Cargar todas las imágenes y sonidos
        const scene = this.game.scene.scenes[0];

        // Añadir un mensaje de depuración
        console.log('Preload iniciado - Cargando assets');

        // Ruta correcta para apuntar a la carpeta wwwroot
        const baseUrl = '/assets/';

        // Cargar imágenes
        scene.load.image("fondo", baseUrl + "images/fondo_wb.png");
        scene.load.image("buzo", baseUrl + "images/buzo.png");
        scene.load.image("burbuja", baseUrl + "images/burbuja.png");
        scene.load.image("coral", baseUrl + "images/coral.png");
        scene.load.image("tiburon", baseUrl + "images/tibu.png");
        scene.load.image("flecha_arriba", baseUrl + "images/arrow_up.png");
        scene.load.image("flecha_abajo", baseUrl + "images/arrow_down.png");
        scene.load.image('reset-button', baseUrl + "images/reset.png");

        // Cargar música
        scene.load.audio("musica", baseUrl + "sounds/st.mp3");

        // Eventos de carga
        scene.load.on('progress', function (value) {
            console.log('Progreso de carga: ' + (value * 100) + '%');
        });

        scene.load.on('complete', function () {
            console.log('Carga completa!');
        });

        scene.load.on('error', function (file) {
            console.error('Error cargando archivo: ' + file.src);
        });
    }

    create() {
        const scene = this.game.scene.scenes[0];

        // Configurar fondo
        scene.add.image(this.config.width / 2, this.config.height / 2, "fondo").setDisplaySize(this.config.width, this.config.height);

        // Configurar controles
        this.cursors = scene.input.keyboard.createCursorKeys();
        this.spaceKey = scene.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.SPACE);

        // Agregar detección de clic
        scene.input.on('pointerdown', this.handlePointerDown.bind(this));
        scene.input.on('pointerup', this.handlePointerUp.bind(this));

        // Monitorear el espacio
        this.spaceKey.on('down', this.handlePointerDown.bind(this));
        this.spaceKey.on('up', this.handlePointerUp.bind(this));

        // Música
        this.music = scene.sound.add("musica", { loop: true });

        // Crear burbuja (inicialmente invisible)
        this.burbuja = scene.add.image(this.config.width / 2, this.config.height - this.basePosition, "burbuja")
            .setAlpha(0.5)
            .setScale(0.8)
            .setVisible(false);

        // Crear buzo (inicialmente invisible)
        this.buzo = scene.add.image(this.config.width / 2, this.config.height - (this.basePosition - 10), "buzo")
            .setScale(0.6)
            .setVisible(false);

        // Crear la barra de progreso de respiración (inicialmente invisible)
        this.crearBarraRespiracion(scene);

        // Texto para la cuenta regresiva
        this.countdownText = scene.add.text(this.config.width / 2, this.config.height / 2, "", {
            fontSize: '64px',
            fontWeight: 'bold',
            fill: '#ffffff',
            stroke: '#000000',
            strokeThickness: 6
        }).setOrigin(0.5).setVisible(false);

        // Botón de inicio
        const startButton = scene.add.text(this.config.width / 2, this.config.height * 0.8, "COMENZAR", {
            fontSize: '32px',
            fontWeight: 'bold',
            fill: '#ffffff',
            backgroundColor: '#0066ff',
            padding: {
                x: 20,
                y: 10
            },
            borderRadius: 15
        }).setOrigin(0.5).setInteractive({ useHandCursor: true });

        // Evento para el botón de inicio
        startButton.on('pointerdown', () => {
            startButton.setVisible(false);
            this.startGame(scene);
        });

        // Botón de reinicio (inicialmente invisible)
        const resetButton = scene.add.image(this.config.width - 80, 40, 'reset-button')
            .setOrigin(0.5)
            .setInteractive({ useHandCursor: true })
            .setVisible(false);

        // Evento para el botón de reinicio
        resetButton.on('pointerdown', () => {
            this.resetGame(scene);
        });

        // Guardar referencia al botón de reinicio
        this.resetButton = resetButton;
    }

    update(time, delta) {
        if (!this.hasStarted) return;

        // Si estamos en la cuenta regresiva, no hacer nada más
        if (this.countdownText.visible) return;

        // Convertir delta de ms a segundos
        const deltaSeconds = delta / 1000;

        // Actualizar el ciclo de respiración
        this.updateRespiracionCycle(this.game.scene.scenes[0], deltaSeconds);

        // Actualizar posición de los obstáculos
        this.updateObstacles(this.game.scene.scenes[0], deltaSeconds);
    }

    startGame(scene) {
        this.hasStarted = true;

        // Mostrar cuenta regresiva
        this.countdownText.setVisible(true);
        this.countdownText.setText("3");

        // Parar música si está sonando
        if (this.music.isPlaying) {
            this.music.stop();
        }

        // Configurar temporizador para cuenta regresiva
        scene.time.delayedCall(1000, () => {
            this.countdownText.setText("2");

            scene.time.delayedCall(1000, () => {
                this.countdownText.setText("1");

                scene.time.delayedCall(1000, () => {
                    this.countdownText.setText("Inhala");

                    scene.time.delayedCall(1000, () => {
                        // Ocultar texto de cuenta regresiva e iniciar juego
                        this.countdownText.setVisible(false);

                        // Mostrar elementos del juego
                        this.burbuja.setVisible(true);
                        this.buzo.setVisible(true);
                        this.mostrarElementosRespiracion(true);
                        this.resetButton.setVisible(true);

                        // Iniciar música
                        this.music.play();

                        // Iniciar ciclo de respiración
                        this.startRespiracionCycle();
                    });
                });
            });
        });
    }

    resetGame(scene) {
        // Detener música
        this.music.stop();

        // Reiniciar variables
        this.hasStarted = false;
        this.barraLlenando = true;
        this.respiracionBarraProgreso = 0;
        this.bubbleOffset = 0;
        this.nextIsCoral = true;
        this.lastObstacleTime = 0;

        // Limpiar obstáculos
        this.obstacles.forEach(obs => {
            if (obs.sprite) obs.sprite.destroy();
        });
        this.obstacles = [];

        // Ocultar elementos del juego
        this.burbuja.setVisible(false);
        this.buzo.setVisible(false);
        this.mostrarElementosRespiracion(false);
        this.resetButton.setVisible(false);

        // Reiniciar posiciones
        this.burbuja.y = this.config.height - this.basePosition;
        this.buzo.y = this.config.height - (this.basePosition - 10);

        // Crear nuevo botón de inicio
        const startButton = scene.add.text(this.config.width / 2, this.config.height * 0.8, "COMENZAR", {
            fontSize: '32px',
            fontWeight: 'bold',
            fill: '#ffffff',
            backgroundColor: '#0066ff',
            padding: {
                x: 20,
                y: 10
            },
            borderRadius: 15
        }).setOrigin(0.5).setInteractive({ useHandCursor: true });

        startButton.on('pointerdown', () => {
            startButton.destroy();
            this.startGame(scene);
        });
    }

    handlePointerDown() {
        if (!this.hasStarted || this.countdownText.visible) return;

        this.isInhaling = true;
        this.bubbleOffset = 300; // El buzo sube cuando inhala

        // Animar el movimiento hacia arriba
        if (this.burbuja && this.burbuja.scene) {
            this.burbuja.scene.tweens.add({
                targets: [this.burbuja, this.buzo],
                y: "-=450",
                duration: 5000,
                ease: 'Sine.easeOut'
            });
        }
    }

    handlePointerUp() {
        if (!this.hasStarted || this.countdownText.visible) return;

        this.isInhaling = false;
        this.bubbleOffset = 0; // El buzo vuelve a la posición base al exhalar

        // Animar el movimiento hacia abajo
        if (this.burbuja && this.burbuja.scene) {
            this.burbuja.scene.tweens.add({
                targets: [this.burbuja, this.buzo],
                y: "+=450",
                duration: 5000,
                ease: 'Sine.easeIn'
            });
        }
    }

    crearBarraRespiracion(scene) {
        // Grupo para los elementos de la barra
        const barraGroup = scene.add.group();

        // Fondo de la barra
        this.barraFondo = scene.add.rectangle(
            this.config.width / 2,
            40,
            this.config.width * 0.7,
            30,
            0x000000,
            0.3
        ).setOrigin(0.5);

        // Progreso de la barra (inicialmente vacío)
        this.barraGrafica = scene.add.rectangle(
            (this.config.width / 2) - (this.config.width * 0.7) / 2 + 2,
            40,
            0,
            26,
            0x0088ff,
            0.7
        ).setOrigin(0, 0.5);

        // Texto de la barra
        this.textoRespiracion = scene.add.text(
            this.config.width / 2,
            40,
            "Inhala",
            {
                fontSize: '18px',
                fontWeight: 'bold',
                fill: '#ffffff'
            }
        ).setOrigin(0.5);

        // Icono de flecha (placeholder)
        this.iconoRespiracion = scene.add.image(
            this.config.width / 2 - 50,
            40,
            "flecha_arriba"
        ).setScale(0.2);

        // Agregar todos los elementos al grupo
        barraGroup.add(this.barraFondo);
        barraGroup.add(this.barraGrafica);
        barraGroup.add(this.textoRespiracion);
        barraGroup.add(this.iconoRespiracion);

        // Ocultar inicialmente
        barraGroup.setVisible(false);

        // Guardar referencia
        this.barraGroup = barraGroup;
    }

    mostrarElementosRespiracion(visible) {
        this.barraFondo.setVisible(visible);
        this.barraGrafica.setVisible(visible);
        this.textoRespiracion.setVisible(visible);
        this.iconoRespiracion.setVisible(visible);
    }

    startRespiracionCycle() {
        this.barraLlenando = true;
        this.respiracionBarraProgreso = 0;

        // Generar el primer obstáculo al inicio
        this.spawnObstacle();
    }

    updateRespiracionCycle(scene, deltaSeconds) {
        // Actualizar el temporizador interno
        this.respiracionTimer += deltaSeconds;

        if (this.barraLlenando) {
            // Fase de inhalación (5 segundos)
            this.respiracionBarraProgreso += deltaSeconds / 5; // 5 segundos para llenar la barra

            if (this.respiracionBarraProgreso >= 1) {
                this.respiracionBarraProgreso = 1;
                this.barraLlenando = false; // Cambiar a exhalación
                this.cycleCompleted = true;

                // Cambiar el texto e icono
                this.textoRespiracion.setText("Exhala");
                this.iconoRespiracion.setTexture("flecha_abajo");

                // Cambiar el color de la barra
                this.barraGrafica.fillColor = 0x00ff00; // Verde para exhalar

                // Generar obstáculo cuando la barra está llena
                this.spawnObstacle();
            }
        } else {
            // Fase de exhalación (5 segundos)
            this.respiracionBarraProgreso -= deltaSeconds / 5; // 5 segundos para vaciar la barra

            if (this.respiracionBarraProgreso <= 0) {
                this.respiracionBarraProgreso = 0;
                this.barraLlenando = true; // Cambiar a inhalación
                this.cycleCompleted = true;

                // Cambiar el texto e icono
                this.textoRespiracion.setText("Inhala");
                this.iconoRespiracion.setTexture("flecha_arriba");

                // Cambiar el color de la barra
                this.barraGrafica.fillColor = 0x0088ff; // Azul para inhalar

                // Generar obstáculo cuando la barra está vacía
                this.spawnObstacle();
            }
        }

        // Actualizar el ancho de la barra de progreso
        const barraAncho = this.config.width * 0.7 - 4; // -4 por el padding
        this.barraGrafica.width = barraAncho * this.respiracionBarraProgreso;
    }

    updateObstacles(scene, deltaSeconds) {
        // Mover todos los obstáculos
        const obstaculosToRemove = [];

        this.obstacles.forEach(obs => {
            // Mover el obstáculo hacia la izquierda
            obs.x -= this.obstacleSpeed * deltaSeconds;

            // Actualizar la posición del sprite
            if (obs.sprite) {
                obs.sprite.x = obs.x;
            }

            // Marcar para eliminar si está fuera de la pantalla
            if (obs.x < -260) { // Ancho del obstáculo
                obstaculosToRemove.push(obs);
            }
        });

        // Eliminar obstáculos que salieron de la pantalla
        obstaculosToRemove.forEach(obs => {
            const index = this.obstacles.indexOf(obs);
            if (index > -1) {
                this.obstacles.splice(index, 1);
                if (obs.sprite) {
                    obs.sprite.destroy();
                }
            }
        });
    }

    spawnObstacle() {
        const esCoral = this.nextIsCoral;
        this.nextIsCoral = !this.nextIsCoral; // Alternar para el siguiente

        // Crear un nuevo objeto obstáculo
        const nuevoObstaculo = {
            x: this.config.width,
            esCoral: esCoral,
            sprite: null
        };

        // Crear el sprite según el tipo
        if (this.burbuja && this.burbuja.scene) {
            const scene = this.burbuja.scene;

            if (esCoral) {
                // Coral (parte inferior)
                nuevoObstaculo.sprite = scene.add.image(
                    nuevoObstaculo.x,
                    this.config.height - 260, // Ajustar según necesidad
                    "coral"
                ).setOrigin(0, 0);
            } else {
                // Tiburón (parte superior)
                nuevoObstaculo.sprite = scene.add.image(
                    nuevoObstaculo.x,
                    50, // Ajustar según necesidad
                    "tiburon"
                ).setOrigin(0, 0);
            }
        }

        // Agregar a la lista de obstáculos
        this.obstacles.push(nuevoObstaculo);
    }
}

// Función para inicializar el juego cuando se requiera
function initBuzoGame(containerId) {
    // Comprobar si Phaser está disponible
    if (typeof Phaser === 'undefined') {
        console.error('Phaser no está disponible. Asegúrate de incluir la biblioteca Phaser antes de inicializar el juego.');
        return null;
    }

    // Verificar si el contenedor existe
    const container = document.getElementById(containerId);
    if (!container) {
        console.error(`El contenedor con ID "${containerId}" no existe en el documento.`);
        return null;
    }

    console.log('Inicializando BuzoGame en el contenedor:', containerId);

    // Crear e inicializar el juego
    return new BuzoGame(containerId);
}