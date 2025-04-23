namespace OK.Models
{
    public class ResultadosTemp
    {
        public double Media { get; set; }
        public double Porcentaje { get; set; }
        public string Nivel { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;

        public List<int> Estresores { get; set; } = new();
        public List<int> Sintomas { get; set; } = new();
        public List<int> Afrontamiento { get; set; } = new();
    }
}
