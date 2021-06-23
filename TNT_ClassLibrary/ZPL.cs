using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TNT_ClassLibrary
{
	//
	// CLASSE PRINCIPALE
	//
	public class ZPL
	{
		public static async Task<string> getZPL(string consignmentno)
		{
			string xmlString = $@"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:ser='http://services.resi.tnt.com'>
								   <soapenv:Header/>
								   <soapenv:Body>
									  <ser:getPDFLabel>
										 <inputXml><![CDATA[<?xml version='1.0' encoding='utf-8'?>
											<shipment xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:noNamespaceSchemaLocation='W:ExpressLabel\Internazionale\routinglabel.xsd'>
												<software>
													<application>MYRTL</application> 
													<version>1.0</version> 
												</software>
												<security>
													<customer>U00723</customer>
													<user>DYNAPARTS</user>
													<password>12715s</password>
													<langid>IT</langid>
												</security>
												<consignment action='R'>
													<senderAccId>20005536</senderAccId>
													<consignmentno>{consignmentno}</consignmentno>
													<consignmenttype>C</consignmenttype>
													<PrintInstrDocs>N</PrintInstrDocs>
													<labelType>T</labelType>
												</consignment>
											</shipment>]]></inputXml>
									  </ser:getPDFLabel>
								   </soapenv:Body>
								</soapenv:Envelope>";

			//
			// INVIO RICHIESTA && ESTRAZIONE RISPOSTA 
			//
			HttpResponseMessage response = await RichiestaPostXML("https://www.mytnt.it/ResiService/services/ResiServiceImpl", xmlString);
			XmlDocument content = new();
			content.LoadXml(await response.Content.ReadAsStringAsync());

			//
			//	ESTRAZIONE ZPL
			//
			XmlNamespaceManager nsmanager = new(content.NameTable);
			nsmanager.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
			nsmanager.AddNamespace("wsa", "http://www.w3.org/2005/08/addressing");
			nsmanager.AddNamespace("p70", "http://services.resi.tnt.com");

			string zpl = PDFtoZPL.Conversion.ConvertPdfPage(content.SelectSingleNode("/soapenv:Envelope/soapenv:Body/p70:getPDFLabelResponse/getPDFLabelReturn/binaryDocument", nsmanager).InnerText);
			return zpl;
		}

		public static async Task<HttpResponseMessage> RichiestaPostXML(string baseUrl, string xmlString)
		{
			using (var httpClient = new HttpClient())
			{
				var httpContent = new StringContent(xmlString, Encoding.UTF8, "text/xml");
				httpContent.Headers.Add("SOAPAction", "getPDFLabel");

				return await httpClient.PostAsync(baseUrl, httpContent);
			}
		}
	}
}