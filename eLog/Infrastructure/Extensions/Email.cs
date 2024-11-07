using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using eLog.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using libeLog.Extensions;

namespace eLog.Infrastructure.Extensions
{
    public class Email
    {

        public static void SendEmail(string subject, string body, string smtpAddress, int portNumber, bool enableSSL, string emailFrom, string password, List<string> emailTo)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFrom, "Уведомлятель");
                foreach (string s in emailTo) { mail.To.Add(s); }
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    smtp.Credentials = new NetworkCredential(emailFrom, password);
                    smtp.EnableSsl = enableSSL;
                    smtp.Send(mail);
                }
            }
        }

        public static bool SendLongSetupNotify(Part part)
        {
            try
            {
                var smtpPwd = Environment.GetEnvironmentVariable("NOTIFY_SMTP_PWD", EnvironmentVariableTarget.User);
                if (string.IsNullOrEmpty(smtpPwd)) throw new Exception("SMTP пароль не установлен.");
                var emailBody = new StringBuilder();

                emailBody.Append($@"
                <html>
                <body style=""font-family: Calibri, sans-serif; color: #333;"">
                    <div style=""max-width: 320px; margin: 0 auto; padding: 15px; border: 1px solid #ddd; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
        
                        <p style=""font-size: 18px; font-weight: bold; color: #d9534f; text-align: center;"">
                            Внимание: процесс наладки превысил {AppSettings.Instance.TimerForNotify.FormattedHours()}!
                        </p>

                        <hr style=""border: none; border-top: 1px solid #ddd; margin: 15px 0;"">

                        <div style=""margin-bottom: 15px;"">
                            <p style=""margin: 6px 0;""><strong>Станок:</strong> {AppSettings.Instance.Machine.Name}</p>
                            <p style=""margin: 6px 0;""><strong>Оператор:</strong> {part.Operator.FullName}</p>
                            <p style=""margin: 6px 0;""><strong>Деталь:</strong> {part.FullName.TrimLen(70)}</p>
                            <p style=""margin: 6px 0;""><strong>Установка:</strong> {part.Setup}</p>
                            <p style=""margin: 6px 0;""><strong>М/Л:</strong> {part.Order}</p>
                            <p style=""margin: 6px 0;""><strong>Начало наладки:</strong> {part.StartSetupTime}</p>
                        </div>
                ");
                if (part.DownTimes.Any())
                {
                    emailBody.Append(@"
                        <div style=""margin-bottom: 0px;"">
                            <p style=""margin: 0px 0; font-size: 16px; font-weight: bold; color: #120085;"">Простои:</p>
                        ");

                    foreach (var dt in part.DownTimes)
                    {
                        emailBody.Append($@"
                            <p style=""margin: 6px 0;""><strong>{dt.Name}: </strong>{dt.StartTime:t} - {(dt.EndTime == DateTime.MinValue ? "..." : $"{dt.EndTime:t}")}</p>
                        ");
                    }

                    emailBody.Append("</div>");
                }

                emailBody.Append(@"
                        <hr style=""border: none; border-top: 1px solid #ddd; margin: 15px 0;"">
                        <p style=""font-size: 11px; text-align: center; color: #777; margin-top: 20px;"">
                            Это сообщение сформировано автоматически, не отвечайте на него.
                        </p>
                    </div>
                </body>
                </html>");
                SendEmail("Уведомление о длительной наладке", emailBody.ToString(), AppSettings.Instance.SmtpAddress, AppSettings.Instance.SmtpPort, true, AppSettings.Instance.SmtpUsername, smtpPwd, AppSettings.LongSetupsMailRecievers);

                return true;
            } 
            catch (Exception ex)
            {
                Util.WriteLog(ex);
                return false;
            }
        }

        public static bool SendToolSearchComment(Part part, string comment)
        {
            try
            {
                var smtpPwd = Environment.GetEnvironmentVariable("NOTIFY_SMTP_PWD", EnvironmentVariableTarget.User);
                if (string.IsNullOrEmpty(smtpPwd)) throw new Exception("SMTP пароль не установлен.");
                var emailBody = new StringBuilder();

                emailBody.Append($@"
                <html>
                <body style=""font-family: Calibri, sans-serif; color: #333;"">
                    <div style=""max-width: 320px; margin: 0 auto; padding: 15px; border: 1px solid #ddd; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);"">
        
                        <p style=""font-size: 18px; font-weight: bold; color: #d9534f; text-align: center;"">
                            Внимание: оператору не хватает инструмента!
                        </p>

                        <hr style=""border: none; border-top: 1px solid #ddd; margin: 15px 0;"">

                        <div style=""margin-bottom: 15px;"">
                            <p style=""margin: 6px 0;""><strong>Станок:</strong> {AppSettings.Instance.Machine.Name}</p>
                            <p style=""margin: 6px 0;""><strong>Оператор:</strong> {part.Operator.FullName}</p>
                            <p style=""margin: 6px 0;""><strong>Деталь:</strong> {part.FullName.TrimLen(70)}</p>
                        </div>

                        <div style=""margin-bottom: 15px;"">
                            <p style=""margin: 6px 0;""><strong>Что нужно:</strong> {comment}</p>
                        </div>
                ");
                

                emailBody.Append(@"
                        <hr style=""border: none; border-top: 1px solid #ddd; margin: 15px 0;"">
                        <p style=""font-size: 11px; text-align: center; color: #777; margin-top: 20px;"">
                            Это сообщение сформировано автоматически, не отвечайте на него.
                        </p>
                    </div>
                </body>
                </html>");
                SendEmail("Уведомление об инструменте", emailBody.ToString(), AppSettings.Instance.SmtpAddress, AppSettings.Instance.SmtpPort, true, AppSettings.Instance.SmtpUsername, smtpPwd, AppSettings.ToolSearchMailRecievers);

                return true;
            }
            catch (Exception ex)
            {
                Util.WriteLog(ex);
                return false;
            }
        }
    }
}
