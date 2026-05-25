namespace GestionFormacion.Models
{
    public class Asistencia
    {
        public int Id { get; set; }

        public int EmpleadoId { get; set; }
        public Empleado? Empleado { get; set; }

        public int CursoId { get; set; }
        public Curso? Curso { get; set; }

        public DateTime Fecha { get; set; }

        public bool Asistio { get; set; }
    }
}