namespace GestionFormacion.Models
{
    public class Inscripcion
    {
        public int Id { get; set; }

        public int EmpleadoId { get; set; }
        public Empleado Empleado { get; set; }

        public int CursoId { get; set; }
        public Curso Curso { get; set; }

        public DateTime FechaInscripcion { get; set; }

        public string Estado { get; set; } = "Pendiente";
    }
}
