using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("1dfc45c5601b32", "a53223b438010a"),
                EnableSsl = true
            };
            client.Send("from@example.com", "to@example.com", "Hello world", "testbody");
            Console.WriteLine("Sent");
            Console.ReadLine();
        }
    }
}