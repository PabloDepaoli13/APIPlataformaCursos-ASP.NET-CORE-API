using AppPlataformaAprendizaje.DAL.Implementations;
using AppPlataformaAprendizaje.DTO;
using AppPlataformaCursos.DAL.DataContext;
using AppPlataformaCursos.DAL.Interfaces;
using AppPlataformaCursos.DTO;
using AppPlataformaCursos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using XSystem.Security.Cryptography;

namespace AppPlataformaCursos.DAL.Implementations
{
    public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
    {
        private string _claveSecreta;
        private AplicationDbContext _context;
       
        public UsuarioRepository(AplicationDbContext context, IConfiguration config) : base(context)
        {
            _claveSecreta = config.GetValue<string>("APIConfig:ClaveSecreta");
            _context= context;
            
        }

        public async Task<UsuarioLoginRespuestaDTO> Login(UsuarioLoginDTO usuario)
        {

            var passwordEncryptado = MD5Encrypter(usuario.Password);

            var key = Encoding.ASCII.GetBytes(_claveSecreta);

            var usuarioEncontrado = await _context.Usuarios.FirstOrDefaultAsync(e => e.NombreUsuario.ToLower() == usuario.NombreUsuario.ToLower() && e.Password == passwordEncryptado);

            if (usuarioEncontrado == null)
            {
                return new UsuarioLoginRespuestaDTO()
                {
                    Token = "",
                    Usuario = null
                };
            }

            var manejadorToken = new JwtSecurityTokenHandler();

            var descriptorToken = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name ,  usuarioEncontrado.NombreUsuario.ToString()),
                    new Claim(ClaimTypes.Role , usuarioEncontrado.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new (new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                
            };

            var token = manejadorToken.CreateToken(descriptorToken);

            var usuarioLoginRespuestaDTO = new UsuarioLoginRespuestaDTO()
            {
                Token = manejadorToken.WriteToken(token),
                Usuario = usuarioEncontrado
            };

            return usuarioLoginRespuestaDTO;

            
        }

        public async Task<bool> RegisterUsuario(UsuarioRegistroDTO usuario)
        {
            var result = false;

            var contraseñaEncriptada = MD5Encrypter(usuario.Password);

            var usuarioNuevo = new Usuario()
            {
                NombreUsuario = usuario.NombreUsuario,
                Nombre= usuario.Nombre,
                Password= contraseñaEncriptada,
                Role =usuario.Role,
            };

            _context.Usuarios.Add(usuarioNuevo);
            usuarioNuevo.Password= contraseñaEncriptada;
            result = await _context.SaveChangesAsync() > 0;
            return result;

        }

        public async Task<bool> UniqueUsuario(string nombreUsuario)
        {
            var usuarioEncontrado = await _context.Usuarios.FirstOrDefaultAsync(e => e.NombreUsuario == nombreUsuario);
            if (usuarioEncontrado == null) return true;
            return false;
          
        }

        public async Task<bool> UpdateUsuario(Usuario usuario)
        {
            var result = false;

            var usuarioViejo = await _context.Usuarios.FindAsync(usuario.Id);

            if (usuarioViejo == null)
            {
                return false;
            }

            var contraseñaEncriptada = MD5Encrypter(usuario.Password);

            usuarioViejo.NombreUsuario = usuario.NombreUsuario ?? usuarioViejo.NombreUsuario;
            usuarioViejo.Nombre = usuario.Nombre ?? usuarioViejo.Nombre;
            usuarioViejo.Password = contraseñaEncriptada ?? usuarioViejo.Password;
            usuarioViejo.Role = usuario.Role ?? usuarioViejo.Role;

            _context.Entry(usuarioViejo).State = EntityState.Modified;
           
            result = await _context.SaveChangesAsync() > 0;
            return result;
        }

        public static string MD5Encrypter(string password)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] info = Encoding.UTF8.GetBytes(password);
            info = md5.ComputeHash(info);
            var passwordEncrypted = "";

            for (int i = 0; i < info.Length; i++)
            {
                passwordEncrypted += info[i].ToString("x2").ToLower();
            }

            return passwordEncrypted;
        }
    }
}
