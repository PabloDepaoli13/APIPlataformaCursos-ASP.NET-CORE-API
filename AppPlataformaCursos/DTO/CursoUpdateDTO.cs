﻿namespace AppPlataformaCursos.DTO
{
    public class CursoUpdateDTO
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string Precio { get; set; }

        public string Duracion { get; set; }

        public string FechaInicio { get; set; }

        public int InstructorID { get; set; }
    }
}
