using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendTorneosS.Contexto;
using BackendTorneosS.Entidades;
// JWT
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BackendTorneosS.Servicios;
using BCrypt.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;


namespace BackendTorneosS.Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly DBContext _context;

        public UsuarioController(DBContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var client = new HttpClient();

            var secret = Environment.GetEnvironmentVariable("secret");
            var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
            {"secret", secret},
            {"response", login.captcha}
                }));

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<RecaptchaResponse>(json);

            if (!result.success)
                return BadRequest("Falló la validación humana");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.strCorreo == login.strCorreo);

            if (usuario == null || !usuario.bitEstatus)
            {
                return Unauthorized(new { message = "Credenciales inválidas." });
            }

            // Verificar contraseña hasheada
            bool passwordValida = BCrypt.Net.BCrypt.Verify(login.strPass, usuario.strPass);

            if (!passwordValida)
            {
                return Unauthorized(new { message = "Credenciales inválidas." });
            }


            usuario.datFechaUltimoLogin = DateTime.UtcNow;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            var key = Environment.GetEnvironmentVariable("JWT__KEY");
            var issuer = Environment.GetEnvironmentVariable("JWT__ISSUER");
            var audience = Environment.GetEnvironmentVariable("JWT__AUDIENCE");
            var expireMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT__EXPIREMINUTES") ?? "60");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jti = Guid.NewGuid().ToString();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(ClaimTypes.NameIdentifier, usuario.intUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuario.strCorreo.ToString()),
                new Claim(ClaimTypes.Role, (usuario.isAdmin ?? false) ? "Admin" : "User")
      };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);



            var tokenHash = BCrypt.Net.BCrypt.HashPassword(tokenString); // se recomienda hashear

            var UsuarioSesion = new UsuarioSesion
            {
                intUsuario = usuario.intUsuario,
                strJti = jti,
                strTokenHash = tokenHash,
                strDeviceInfo = Request.Headers["User-Agent"].ToString(),
                strIPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                datFechaCreado = DateTime.UtcNow,
                datExpiracion = DateTime.UtcNow.AddMinutes(expireMinutes),
                bitRevoked = false
            };

            _context.UsuarioSesion.Add(UsuarioSesion);
            await _context.SaveChangesAsync();

            Response.Cookies.Append("access_token", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes)
            });

            return Ok(new
            {
                usuario = new
                {
                    usuario.strNombre,
                    usuario.strApellidoPaterno,
                    usuario.strApellidoMaterno,
                    usuario.strCorreo,
                    usuario.bitEstatus,
                    usuario.datFechaRegistro,
                    usuario.datFechaActualizacion,
                    usuario.strMoneda,
                    usuario.datFechaUltimoLogin,
                    isAdmin = usuario.isAdmin,
                    expireMinutes
                }
            });
        }

        [Authorize]
        [HttpGet("perfil")]
        public IActionResult GetUsuarioActual()
        {

            var intUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var strCorreo = User.FindFirst(ClaimTypes.Email)?.Value;
            var isAdmin = User.FindFirst(ClaimTypes.Role)?.Value;

            var usuarioActual = _context.Usuarios.FirstOrDefault(x => x.intUsuario == int.Parse(intUsuario));
            return Ok(new
            {
                tseckn_144 = intUsuario,
                strCorreo = strCorreo,
                strNombre = usuarioActual.strNombre,
                strApellidoPaterno = usuarioActual.strApellidoPaterno,
                strApellidoMaterno = usuarioActual.strApellidoMaterno,
                bitEstatus = usuarioActual.bitEstatus,
                datFechaRegistro = usuarioActual.datFechaRegistro,
                datFechaActualizacion = usuarioActual.datFechaActualizacion,
                datFechaUltimoLogin = usuarioActual.datFechaUltimoLogin,
                isAdmin = usuarioActual.isAdmin
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var sesion = await _context.UsuarioSesion.FirstOrDefaultAsync(s => s.strJti == jti);
            if (sesion != null)
            {
                sesion.bitRevoked = true;
                await _context.SaveChangesAsync();
            }

            Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });
            Response.Cookies.Append("access_token", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UnixEpoch
            });

            return Ok("Sesión cerrada.");
        }


        [HttpPost("validar-sesion")]
        public async Task<bool> ValidarSesion(string token, DBContext _context)
        {
            var sesiones = await _context.UsuarioSesion.ToListAsync();

            var sesionValida = sesiones.FirstOrDefault(s =>
                !s.bitRevoked &&
                s.datExpiracion > DateTime.UtcNow &&
                BCrypt.Net.BCrypt.Verify(token, s.strTokenHash));

            return sesionValida != null;
        }


        [HttpPost("restaurar-pass")]
        public async Task<IActionResult> RestaurarPass([FromBody] LoginDTO modelo)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.strCorreo == modelo.strCorreo);
            if (usuario == null)
            {
                return BadRequest("Usuario no encontrado");
            };

            var token = Guid.NewGuid().ToString();
            usuario.strTokenRecuperacion = token;
            usuario.datFechaToken = DateTime.Now;

            await _context.SaveChangesAsync();

            var emailService = new EmailService();
            await emailService.EnviarCorreo(modelo.strCorreo, token);

            return Ok("Correo enviado!");

        }

        [HttpPost("cambiar-pass")]
        public async Task<IActionResult> CambiarContrasena([FromBody] ResetPassDTO modelo)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.strTokenRecuperacion == modelo.Token);

            if (usuario == null || usuario.datFechaToken == null)
            {
                return BadRequest("Token inválido o expirado.");
            }

            if ((DateTime.Now - usuario.datFechaToken.Value).TotalMinutes > 30)
            {
                return BadRequest("El token ha expirado.");
            }


            usuario.strPass = BCrypt.Net.BCrypt.HashPassword(modelo.NuevaContrasena);
            usuario.strTokenRecuperacion = null;
            usuario.datFechaToken = null;

            await _context.SaveChangesAsync();

            return Ok("Contraseña actualizada con éxito.");
        }

        // GET: api/Usuario
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuarios>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        [Authorize(Roles = "administrador")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuarios>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        //POST: api/Usuario
        [HttpPost]
        public async Task<ActionResult<Usuarios>> PostUsuario(Usuarios usuario)
        {
            var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.strCorreo == usuario.strCorreo);

            if (usuarioExistente != null)
            {
                return BadRequest("El correo ya esta registrado.");
            }
            usuario.strPass = BCrypt.Net.BCrypt.HashPassword(usuario.strPass);
            usuario.isAdmin = false;
            if (usuario.datFechaRegistro == default || usuario.datFechaRegistro < new DateTime(1753, 1, 1))
                usuario.datFechaRegistro = DateTime.Now;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Created($"{usuario.intUsuario}", null); 
        }

        // PUT: api/Usuario/5
        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutUsuario(int id, Usuarios usuario)
        {
            if (id != usuario.intUsuario)
                return BadRequest();

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Usuarios.Any(e => e.intUsuario == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
