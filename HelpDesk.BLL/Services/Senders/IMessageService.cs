using HelpDesk.Models.Enums;
using HelpDesk.Models.ViewModels;
using System.Threading.Tasks;

namespace HelpDesk.BLL.Services.Senders
{
    public interface IMessageService
    {
        MessageStates MessageState { get; }

        Task SendAsync(EmailModel message, params string[] contacts);
        void Send(EmailModel message, params string[] contacts);
    }
}
