using HelpDesk.Models.Enums;
using HelpDesk.Models.ViewModels;
using System.Threading.Tasks;

namespace HelpDesk.BLL.Services.Senders
{
    public interface IMessageService
    {
        MessageStates MessageState { get; }

        Task SendAsync(MailModel message, params string[] contacts);
        void Send(MailModel message, params string[] contacts);
    }
}
