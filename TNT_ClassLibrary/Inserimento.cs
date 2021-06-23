using System;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TNT_ClassLibrary
{
	public class Inserimento
    {
		private static DataTable args;

		//
		//	FUNZIONI DI FORMATTAZIONE
		//
		private static char[] FormatPhone(string num)
        {
			char[] numArray = num.ToCharArray();

			if (numArray[0] == '+')
            {
				char[] newNumArray = new char[20] { '0', '0', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.', '.' };
				numArray = numArray.Where((val, idx) => idx != 0).ToArray();
				numArray.CopyTo(newNumArray, 2);

				foreach (char c in newNumArray)
				{
					if (c != '0' && c != '1' && c != '2' && c != '3' && c != '4' && c != '5' && c != '6' && c != '7' && c != '8' && c != '9')
					{
						int cIndex = Array.IndexOf(newNumArray, c);
						newNumArray = newNumArray.Where((val, idx) => idx != cIndex).ToArray();
					}
				}

				return newNumArray;

			} else
            {
				foreach (char c in numArray)
				{
					if (c != '0' && c != '1' && c != '2' && c != '3' && c != '4' && c != '5' && c != '6' && c != '7' && c != '8' && c != '9')
					{
						int cIndex = Array.IndexOf(numArray, c);
						numArray = numArray.Where((val, idx) => idx != cIndex).ToArray();
					}
				}

				return numArray;
			}
        }

		private static string FormatMisure(string nome, string formato, int i = 0)
        {
			float misura = float.Parse((string)args.Rows[i][nome]);
			string strMisura = misura.ToString(formato);
			char[] strArrMisura = FormatPhone(strMisura);
			strMisura = new string(strArrMisura);

			return strMisura;
		}

		public static async Task<XmlDocument> NuovoInserimento(DataTable argsToPass)
        {
			args = argsToPass;

			//
			//	FORMATTAZIONE DI ALCUNI VALORI
			//
			
			//	Telefono
			char[] fullPhone = FormatPhone((string)args.Rows[0]["NTELEFONO"]);

			string phonePrefix = "";
			string phoneNumber = "";
			for (int i = 0; i < fullPhone.Length; i++)
            {
				if (i >= 4)
                {
					phoneNumber += fullPhone[i];
				} else
                {
					phonePrefix += fullPhone[i];
                }
            }

			// Peso Spedizione
			float pesosped = float.Parse((string)args.Rows[0]["PESOSPED"]);
			pesosped *= 1000;
			string actualweight = pesosped.ToString();

			// Data di Ritiro
			char[] collectionDate = FormatPhone((string)args.Rows[0]["NTELEFONO"]);

			string year = "";
			string month = "";
			string day = "";
			for (int i = 0; i < collectionDate.Length; i++)
			{
				if (i == 0 || i == 1)
				{
					day += collectionDate[i];
				}
				else if (i == 2 || i == 3)
				{
					month += collectionDate[i];
				} 
				else if (i >= 4)
                {
					year += collectionDate[i];
				}
			}

			// Numero Colli
			int numeroColli = Int32.Parse((string)args.Rows[0]["NUMCOLLITOT"]);

			string strColli = "";
			for (int i = 0; i < numeroColli; i++)
            {
				// Volume Collo
				string strVolumeCollo = FormatMisure("VOLUME", "0000.000", i);

				// Peso Collo
				string strPesoCollo = FormatMisure("PESOCOLLO_NUM", "00000.000", i);

				// Lunghezza Collo
				string strLunghezzaCollo = FormatMisure("LUNGHEZZA", "000.000", i);

				// Altezza Collo
				string strAltezzaCollo = FormatMisure("ALTEZZA", "000.000", i);

				// Largezza Collo
				string strLarghezzaCollo = FormatMisure("LARGHEZZA", "000.000", i);


				strColli += $@"<dimensions itemaction='I'>
									<itemsequenceno>{i + 1}</itemsequenceno>
									<itemtype>C</itemtype>
									<volume>{strVolumeCollo}</volume>
									<weight>{strPesoCollo}</weight>
									<length>{strLunghezzaCollo}</length>
									<height>{strAltezzaCollo}</height>
									<width>{strLarghezzaCollo}</width>
									<quantity>1</quantity>
								</dimensions>";
            }

			// Importo
			string strImporto = FormatMisure("IMPORTO", "00000000000.00");

			string xmlString = $@"<?xml version='1.0' encoding='utf-8'?>
									<shipment xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:noNamespaceSchemaLocation='W:ExpressLabel\Internazionale\routinglabel.xsd'>
										<software>
											<application>{((string)args.Rows[0]["NNAZIONE"] == "IT" ? "MYRTL" : "MYRTLI")}</application> 
											<version>1.0</version> 
										</software>
										<security>
											<customer>U00723</customer>
											<user>DYNAPARTS</user>
											<password>12715s</password>
											<langid>IT</langid>
										</security>
										<labelType>T</labelType>
										<consignment action='I' hazardous='N' cashondeliver='N' highvalue='N' insurance='{((string)args.Rows[0]["IMPORTO_ASSICURATO"] == "0" ? "N" : "Y")}' international='{((string)args.Rows[0]["NNAZIONE"] == "IT" ? "N" : "Y")}' specialgoods='N'>
											<senderAccId>20005536</senderAccId>
											<PrintInstrDocs>N</PrintInstrDocs>
											<labelType>T</labelType>
											<consignmentno>{args.Rows[0]["COLLO"]}</consignmentno>
											<consignmenttype>C</consignmenttype>
											<actualweight>{actualweight}</actualweight>
											<actualvolume></actualvolume>
											<totalpackages>{args.Rows[0]["NUMCOLLITOT"]}</totalpackages>
											<packagetype>C</packagetype>
											<division>D</division>
											<product>{args.Rows[0]["SERVIZIO_TNT"]}</product>
											<insurancevalue>{args.Rows[0]["IMPORTO_ASSICURATO"]}</insurancevalue>
											<reference>{args.Rows[0]["RIFERIMENTOCLIENTE"]}</reference>
											<collectiondate>{$"{year}{month}{day}"}</collectiondate>
											<invoicevalue>{((string)args.Rows[0]["NNAZIONE"] == "IT" ? null : strImporto)}</invoicevalue>
											<termsofpayment>R</termsofpayment>
											<systemcode>RL</systemcode>
											<systemversion>1.0</systemversion>
											<goodsdesc><![CDATA[{args.Rows[0]["NOTESPEDIZIONE"]}]]></goodsdesc>
											<addresses>
												<address>
													<addressType>S</addressType>
													<vatno>IT01893120368</vatno>
													<addrline1>Via delle nazioni 65</addrline1>
													<addrline2></addrline2>
													<addrline3></addrline3>
													<postcode>41122</postcode>
													<phone1>0039</phone1>
													<phone2>0599780111</phone2>
													<name>Usco s.p.a.</name>
													<country>IT</country>
													<town>Modena</town>
													<province>MO</province>
												</address>
												<address>
													<addressType>R</addressType>
													<addrline1><![CDATA[{args.Rows[0]["NINDIRIZZO"]}]]></addrline1>
													<postcode>{args.Rows[0]["NCAP10"]}</postcode>
													<phone1>{phonePrefix}</phone1>
													<phone2>{phoneNumber}</phone2>
													<name><![CDATA[{args.Rows[0]["NRAGSOCIALE"]}]]></name>
													<country>{args.Rows[0]["NNAZIONE"]}</country>
													<town>{args.Rows[0]["NCITTA"]}</town>
													<province>{args.Rows[0]["NPROVINCIA"] ?? null}</province>
												</address>
											</addresses>
											{strColli}
											<articles>
												<origcountry>IT</origcountry>
											</articles>
										</consignment>
									</shipment>";

			//
			// INVIO RICHIESTA && INOLTRO RISPOSTA
			//
			HttpResponseMessage response = await RichiestaPostXML("https://www.mytnt.it/XMLServices", xmlString);
            XmlDocument content = new();
			content.LoadXml(await response.Content.ReadAsStringAsync());

            return content;
        }

        public static async Task<HttpResponseMessage> RichiestaPostXML(string baseUrl, string xmlString)
        {
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(xmlString, Encoding.UTF8, "text/xml");

				return await httpClient.PostAsync(baseUrl, httpContent);
            }
        }
    }
}