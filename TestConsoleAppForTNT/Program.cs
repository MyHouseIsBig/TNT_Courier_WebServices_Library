using System;
using System.Threading.Tasks;
using System.Xml;
using TNT_ClassLibrary;

namespace TestConsoleAppForTNT
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Richiesta nuovaRichiesta = new();
                XmlDocument risposta = await Richiesta.NuovaRichiestaXml("<data><totalpackages>1</totalpackages><goodsdesc>VARI</goodsdesc></data>");

                Console.WriteLine();
                if (risposta.SelectSingleNode("/Label/Incomplete/Message") != null) 
                {
                    throw new Exception(risposta.SelectSingleNode("/Label/Incomplete/Message").InnerText);
                } else
                {
                    Console.WriteLine(risposta.ToString());
                }
            }
            catch (Exception aex)
            {
                Console.WriteLine($"Exception: {aex.Message}");
            }
        }
    }
}
