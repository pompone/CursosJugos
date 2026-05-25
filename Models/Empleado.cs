using System.ComponentModel.DataAnnotations;

namespace GestionFormacion.Models
{
    public class Empleado
    {
        public int Id { get; set; }

        [Required]
        public string Legajo { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellido { get; set; }

        [Required]
        public string Dni { get; set; }

        public string Sector { get; set; }

        public string Puesto { get; set; }

        public bool Activo { get; set; } = true;
    }
}