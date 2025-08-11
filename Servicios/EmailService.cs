using MailKit.Net.Smtp;
using MimeKit;

namespace BackendTorneosS.Servicios
{
    public class EmailService
    {
        public async Task EnviarCorreo(string destino, string token)
        {
            var passApp = Environment.GetEnvironmentVariable("PASSKEY");
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("EconoWallet", "tovargarciaalejandro25@gmail.com"));
            mensaje.To.Add(new MailboxAddress("", destino));
            mensaje.Subject = "Restablece tu contraseña";

            var link = $"http://localhost:5173/restaurar-pass?token={token}";

            mensaje.Body = new TextPart("html")
            {
                Text = $"<p>Hola,</p><p>Haz clic en el siguiente enlace para restablecer tu contraseña:</p><p><a href=\"{link}\">Restablecer contraseña</a></p><p>Este enlace expirará en 1 hora.</p>"
            };

            using var cliente = new SmtpClient();
            await cliente.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await cliente.AuthenticateAsync("tovargarciaalejandro25@gmail.com", passApp);
            await cliente.SendAsync(mensaje);
            await cliente.DisconnectAsync(true);
        }
    }
}
