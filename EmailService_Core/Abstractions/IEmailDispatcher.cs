using EmailService_Core.Models;

namespace EmailService_Core.Abstractions
{
    public interface IEmailDispatcher
    {
        void SendEmail(Message email);
    }
}
