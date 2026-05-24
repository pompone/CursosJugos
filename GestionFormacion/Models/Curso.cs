using System.ComponentModel.DataAnnotations;

namespace GestionFormacion.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required]
        public string NombreCurso { get; set; }

        public string Descripcion { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public int Horas { get; set; }

        public bool Activo { get; set; } = true;
    }
}