using AppPlataformaAprendizaje.DTO;
using AppPlataformaCursos.DTO;
using AppPlataformaCursos.Models;

namespace AppPlataformaCursos.DAL.Interfaces
{
    public interface IUsuarioRepository : IGenericRepository<Usuario>
    {
        Task<UsuarioLoginRespuestaDTO> Login(UsuarioLoginDTO usuario);

        Task<bool> RegisterUsuario(UsuarioRegistroDTO usuario);

        Task<bool> UniqueUsuario(string nombreUsuario);

        Task<bool> UpdateUsuario(Usuario usuario);
    }
}
