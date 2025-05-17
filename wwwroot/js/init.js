// Configuración básica de Phaser
const config = {
    width: 720,
    height: 540,
    parent: "phaser-container",
    type: Phaser.AUTO,
    scene: {
        preload: preload,
        create: create,
        update: update
    },
    physics: {
        default: "arcade",
    }
};

// Variables globales
let game = new Phaser.Game(config);
let cursors;
let spaceKey;
let buzo;
let burbuja;
let isInhaling = false;
let basePosition = 84; // Posición base del buzo
let bubbleOffset = 168;   // Desplazamiento por respiración

// Variables para los obstáculos
let obstacles = [];
let obstacleSpeed = 112; // Velocidad en píxeles por segundo
let nextIsCoral = true;  // Alternamos coral y tiburón

// Variables para el ciclo de respiración
let respiracionBarraProgreso = 0;
let barraLlenando = true;  // true = inhalar, false = exhalar
let barraGrafica;          // Gráfico de la barra de progreso
let barraFondo;            // Fondo de la barra
let textoRespiracion;      // Texto que indica "Inhala" o "Exhala"
let iconoRespiracion;      // Icono de flecha

// Variables de estado del juego
let hasStarted = false;
let countdown;
let countdownText;
let music;

// Variables de tiempo
let lastObstacleTime = 0;
let respiracionTimer = 0;
let cycleCompleted = false;

function preload() {
    // Cargar todas las imágenes y sonidos
    this.load.image("fondo", "assets/images/fondo_wb.png");
    this.load.image("buzo", "assets/images/buzo.png");
    this.load.image("burbuja", "assets/images/burbuja.png");
    this.load.image("coral", "assets/images/coral.png");
    this.load.image("tiburon", "assets/images/tibu.png");
    this.load.image("flecha_arriba", "assets/images/arrow_up.png");
    this.load.image("flecha_abajo", "assets/images/arrow_down.png");
    
    // Cargar música
    this.load.audio("musica", "assets/sounds/st.mp3");
}

function create() {
    // Configurar fondo
    this.add.image(config.width/2, config.height/2, "fondo").setDisplaySize(config.width, config.height);
    
    // Configurar controles
    cursors = this.input.keyboard.createCursorKeys();
    spaceKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.SPACE);
    
    // Agregar detección de clic
    this.input.on('pointerdown', handlePointerDown);
    this.input.on('pointerup', handlePointerUp);
    
    // Monitorear el espacio
    spaceKey.on('down', handlePointerDown);
    spaceKey.on('up', handlePointerUp);
    
    // Música
    music = this.sound.add("musica", { loop: true });
    
    // Crear burbuja (inicialmente invisible)
    burbuja = this.add.image(config.width/2, config.height - basePosition, "burbuja")
        .setAlpha(0.5)
        .setScale(0.45)
        .setVisible(false);
    
    // Crear buzo (inicialmente invisible)
    buzo = this.add.image(config.width/2, config.height - (basePosition - 10), "buzo")
        .setScale(0.33)
        .setVisible(false);
    
    // Crear la barra de progreso de respiración (inicialmente invisible)
    crearBarraRespiracion(this);
    
    // Texto para la cuenta regresiva
    countdownText = this.add.text(config.width/2, config.height/2, "", {
        fontSize: '64px',
        fontWeight: 'bold',
        fill: '#ffffff',
        stroke: '#000000',
        strokeThickness: 6
    }).setOrigin(0.5).setVisible(false);
    
    // Botón de inicio
    const startButton = this.add.text(config.width/2, config.height * 0.8, "COMENZAR", {
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
        startGame(this);
    });
    
    // Botón de reinicio (inicialmente invisible)
    const resetButton = this.add.text(config.width - 80, 40, "Reiniciar", {
        fontSize: '16px',
        fontWeight: 'bold',
        fill: '#ffffff',
        backgroundColor: '#0066ff',
        padding: {
            x: 10,
            y: 5
        },
        borderRadius: 10
    }).setOrigin(0.5).setScale(0.7).setInteractive({ useHandCursor: true }).setVisible(false);
    
    // Evento para el botón de reinicio
    resetButton.on('pointerdown', () => {
        resetGame(this);
    });
    
    // Guardar referencia al botón de reinicio
    this.resetButton = resetButton;
}

function update(time, delta) {
    if (!hasStarted) return;
    
    // Si estamos en la cuenta regresiva, no hacer nada más
    if (countdownText.visible) return;
    
    // Convertir delta de ms a segundos
    const deltaSeconds = delta / 1000;
    
    // Actualizar el ciclo de respiración
    updateRespiracionCycle(this, deltaSeconds);
    
    // Actualizar posición de los obstáculos
    updateObstacles(this, deltaSeconds);
}

function startGame(scene) {
    hasStarted = true;
    
    // Mostrar cuenta regresiva
    countdownText.setVisible(true);
    countdownText.setText("3");
    
    // Parar música si está sonando
    if (music.isPlaying) {
        music.stop();
    }
    
    // Configurar temporizador para cuenta regresiva
    scene.time.delayedCall(1000, () => {
        countdownText.setText("2");
        
        scene.time.delayedCall(1000, () => {
            countdownText.setText("1");
            
            scene.time.delayedCall(1000, () => {
                countdownText.setText("Inhala");
                
                scene.time.delayedCall(1000, () => {
                    // Ocultar texto de cuenta regresiva e iniciar juego
                    countdownText.setVisible(false);
                    
                    // Mostrar elementos del juego
                    burbuja.setVisible(true);
                    buzo.setVisible(true);
                    mostrarElementosRespiracion(true);
                    scene.resetButton.setVisible(true);
                    
                    // Iniciar música
                    music.play();
                    
                    // Iniciar ciclo de respiración
                    startRespiracionCycle();
                });
            });
        });
    });
}

function resetGame(scene) {
    // Detener música
    music.stop();
    
    // Reiniciar variables
    hasStarted = false;
    barraLlenando = true;
    respiracionBarraProgreso = 0;
    bubbleOffset = 0;
    nextIsCoral = true;
    lastObstacleTime = 0;
    
    // Limpiar obstáculos
    obstacles.forEach(obs => {
        if (obs.sprite) obs.sprite.destroy();
    });
    obstacles = [];
    
    // Ocultar elementos del juego
    burbuja.setVisible(false);
    buzo.setVisible(false);
    mostrarElementosRespiracion(false);
    scene.resetButton.setVisible(false);
    
    // Reiniciar posiciones
    burbuja.y = config.height - basePosition;
    buzo.y = config.height - (basePosition - 10);
    
    // Crear nuevo botón de inicio
    const startButton = scene.add.text(config.width/2, config.height * 0.8, "COMENZAR", {
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
        startGame(scene);
    });
}

function handlePointerDown() {
    if (!hasStarted || countdownText.visible) return;
    
    isInhaling = true;
    bubbleOffset = 168; // El buzo sube cuando inhala
    
    // Animar el movimiento hacia arriba
    if (burbuja && burbuja.scene) {
        burbuja.scene.tweens.add({
            targets: [burbuja, buzo],
            y: "-=250",
            duration: 5000,
            ease: 'Sine.easeOut'
        });
    }
}

function handlePointerUp() {
    if (!hasStarted || countdownText.visible) return;
    
    isInhaling = false;
    bubbleOffset = 0; // El buzo vuelve a la posición base al exhalar
    
    // Animar el movimiento hacia abajo
    if (burbuja && burbuja.scene) {
        burbuja.scene.tweens.add({
            targets: [burbuja, buzo],
            y: "+=250", 
            duration: 5000,
            ease: 'Sine.easeIn'
        });
    }
}

function crearBarraRespiracion(scene) {
    // Grupo para los elementos de la barra
    const barraGroup = scene.add.group();
    
    // Fondo de la barra
    barraFondo = scene.add.rectangle(
        config.width/2,
        40,
        config.width * 0.7,
        30,
        0x000000,
        0.3
    ).setOrigin(0.5);
    
    // Progreso de la barra (inicialmente vacío)
    barraGrafica = scene.add.rectangle(
        (config.width/2) - (config.width * 0.7)/2 + 2,
        40,
        0,
        26,
        0x0088ff,
        0.7
    ).setOrigin(0, 0.5);
    
    // Texto de la barra
    textoRespiracion = scene.add.text(
        config.width/2,
        40,
        "Inhala",
        {
            fontSize: '18px',
            fontWeight: 'bold',
            fill: '#ffffff'
        }
    ).setOrigin(0.5);
    
    // Icono de flecha (placeholder)
    iconoRespiracion = scene.add.image(
        config.width/2 - 50,
        40,
        "flecha_arriba"
    ).setScale(0.2);
    
    // Agregar todos los elementos al grupo
    barraGroup.add(barraFondo);
    barraGroup.add(barraGrafica);
    barraGroup.add(textoRespiracion);
    barraGroup.add(iconoRespiracion);
    
    // Ocultar inicialmente
    barraGroup.setVisible(false);
    
    // Guardar referencia
    scene.barraGroup = barraGroup;
}

function mostrarElementosRespiracion(visible) {
    barraFondo.setVisible(visible);
    barraGrafica.setVisible(visible);
    textoRespiracion.setVisible(visible);
    iconoRespiracion.setVisible(visible);
}

function startRespiracionCycle() {
    barraLlenando = true;
    respiracionBarraProgreso = 0;
    
    // Generar el primer obstáculo al inicio
    spawnObstacle();
}

function updateRespiracionCycle(scene, deltaSeconds) {
    // Actualizar el temporizador interno
    respiracionTimer += deltaSeconds;
    
    if (barraLlenando) {
        // Fase de inhalación (5 segundos)
        respiracionBarraProgreso += deltaSeconds / 5; // 5 segundos para llenar la barra
        
        if (respiracionBarraProgreso >= 1) {
            respiracionBarraProgreso = 1;
            barraLlenando = false; // Cambiar a exhalación
            cycleCompleted = true;
            
            // Cambiar el texto e icono
            textoRespiracion.setText("Exhala");
            iconoRespiracion.setTexture("flecha_abajo");
            
            // Cambiar el color de la barra
            barraGrafica.fillColor = 0x00ff00; // Verde para exhalar
            
            // Generar obstáculo cuando la barra está llena
            spawnObstacle();
        }
    } else {
        // Fase de exhalación (5 segundos)
        respiracionBarraProgreso -= deltaSeconds / 5; // 5 segundos para vaciar la barra
        
        if (respiracionBarraProgreso <= 0) {
            respiracionBarraProgreso = 0;
            barraLlenando = true; // Cambiar a inhalación
            cycleCompleted = true;
            
            // Cambiar el texto e icono
            textoRespiracion.setText("Inhala");
            iconoRespiracion.setTexture("flecha_arriba");
            
            // Cambiar el color de la barra
            barraGrafica.fillColor = 0x0088ff; // Azul para inhalar
            
            // Generar obstáculo cuando la barra está vacía
            spawnObstacle();
        }
    }
    
    // Actualizar el ancho de la barra de progreso
    const barraAncho = config.width * 0.7 - 4; // -4 por el padding
    barraGrafica.width = barraAncho * respiracionBarraProgreso;
}

function updateObstacles(scene, deltaSeconds) {
    // Mover todos los obstáculos
    const obstaculosToRemove = [];
    
    obstacles.forEach(obs => {
        // Mover el obstáculo hacia la izquierda
        obs.x -= obstacleSpeed * deltaSeconds;
        
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
        const index = obstacles.indexOf(obs);
        if (index > -1) {
            obstacles.splice(index, 1);
            if (obs.sprite) {
                obs.sprite.destroy();
            }
        }
    });
}

function spawnObstacle() {
    const esCoral = nextIsCoral;
    nextIsCoral = !nextIsCoral; // Alternar para el siguiente
    
    // Crear un nuevo objeto obstáculo
    const nuevoObstaculo = {
        x: config.width,
        esCoral: esCoral,
        sprite: null
    };
    
    // Crear el sprite según el tipo
    if (burbuja && burbuja.scene) {
        const scene = burbuja.scene;
        
        if (esCoral) {
            // Coral (parte inferior)
            nuevoObstaculo.sprite = scene.add.image(
                nuevoObstaculo.x,
                config.height - 147, // Ajustar según necesidad
                "coral"
            ).setOrigin(0, 0).setScale(0.57);
        } else {
            // Tiburón (parte superior)
            nuevoObstaculo.sprite = scene.add.image(
                nuevoObstaculo.x,
                90, // Ajustar según necesidad
                "tiburon"
            ).setOrigin(0, 0).setScale(0.57);
        }
    }
    
    // Agregar a la lista de obstáculos
    obstacles.push(nuevoObstaculo);
}