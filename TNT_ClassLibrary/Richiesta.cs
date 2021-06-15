using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TNT_ClassLibrary
{
	//
	// CLASSI DI APPOGGIO PER SCORRIMENTO XML
	//
	public static class XmlDocumentExtensions
	{
		public static void IterateThroughAllNodes(
			this XmlDocument doc,
			Action<XmlNode> elementVisitor)
		{
			if (doc != null && elementVisitor != null)
			{
				foreach (XmlNode node in doc.ChildNodes)
				{
					doIterateNode(node, elementVisitor);
				}
			}
		}

		private static void doIterateNode(
			XmlNode node,
			Action<XmlNode> elementVisitor)
		{
			elementVisitor(node);

			foreach (XmlNode childNode in node.ChildNodes)
			{
				doIterateNode(childNode, elementVisitor);
			}
		}
	}

	//
	// CLASSE PRINCIPALE
	//
	public class Richiesta
    {
		public static async Task<XmlDocument> NuovaRichiestaXml(string args)
        {
            string xmlString = @"<?xml version='1.0' encoding='utf-8'?>
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
										<labelType>T</labelType>
										<consignment action='I'>
											<laroseDepot></laroseDepot>
											<senderAccId>20005536</senderAccId>
											<PrintInstrDocs>N</PrintInstrDocs>
											<labelType>T</labelType>
											<consignmentno>giom</consignmentno>
											<consignmenttype>C</consignmenttype>
											<actualweight>00010500</actualweight>
											<actualvolume></actualvolume>
											<totalpackages>1</totalpackages>
											<packagetype>C</packagetype>
											<division>D</division>
											<product>NC</product>
											<vehicle></vehicle>
											<insurancevalue>0000000010000</insurancevalue>
											<insurancecurrency>EUR</insurancecurrency>
											<packingdesc>BOX</packingdesc>
											<reference>UVD-I-2021</reference>
											<collectiondate>20210604</collectiondate>
											<collectiontime></collectiontime>
											<invoicevalue></invoicevalue>
											<invoicecurrency></invoicecurrency>
											<specialinstructions>Attenzione consegnare sempre dopo le 12:00</specialinstructions>
												<options>
													<option></option>
													<option></option>
												</options>
											<termsofpayment>S</termsofpayment>
											<systemcode>RL</systemcode>
											<systemversion>1.0</systemversion>
											<codfvalue>0000000015000</codfvalue>
											<codfcurrency>EUR</codfcurrency>
											<goodsdesc>ABBIGLIAMENTO</goodsdesc>
											<eomenclosure></eomenclosure>
											<eomofferno></eomofferno>
											<eomdivision></eomdivision>
											<eomunification></eomunification>
											<dropoffpoint></dropoffpoint>
											<addresses>
												<address>
													<addressType>S</addressType>
													<vatno></vatno>
													<addrline1>via Roma 124</addrline1>
													<addrline2></addrline2>
													<addrline3></addrline3>
													<postcode>73020</postcode>
													<phone1>011</phone1>
													<phone2>2226111</phone2>
													<name>TEST SPA</name>
													<country>IT</country>
													<town>Modena</town>
													<contactname>Mario Rossi</contactname>
													<province>MO</province>
													<custcountry></custcountry>
												</address>
												<address>
													<addressType>R</addressType>
													<vatno></vatno>
													<addrline1>Via Torino 1</addrline1>
													<addrline2></addrline2>
													<addrline3></addrline3>
													<postcode>00100</postcode>
													<phone1>06</phone1>
													<phone2>111112222</phone2>
													<name>Bianchi SRL</name>
													<country>IT</country>
													<town>Roma</town>
													<contactname>Mario Bianchi</contactname>
													<province>RO</province>
													<custcountry></custcountry>
												</address>
												<address>
													<addressType>C</addressType>
													<vatno></vatno>
													<addrline1></addrline1>
													<addrline2></addrline2>
													<addrline3></addrline3>
													<postcode></postcode>
													<phone1></phone1>
													<phone2></phone2>
													<name></name>
													<country></country>
													<town></town>
													<contactname></contactname>
													<province></province>
													<custcountry></custcountry>
												</address>
												<address>
													<addressType>D</addressType>
													<vatno></vatno>
													<addrline1></addrline1>
													<addrline2></addrline2>
													<addrline3></addrline3>
													<postcode></postcode>
													<phone1></phone1>
													<phone2></phone2>
													<name></name>
													<country></country>
													<town></town>
													<contactname></contactname>
													<province></province>
													<custcountry></custcountry>
												</address>
											</addresses>
											<dimensions>
												<itemsequenceno>1</itemsequenceno>
												<itemtype>C</itemtype>
												<itemreference>0123456789</itemreference>
												<volume></volume>
												<weight>0010000</weight>
												<length></length>
												<height></height>
												<width></width>
												<quantity></quantity>
											</dimensions>
											<articles>
												<tariff></tariff>
												<origcountry>IT</origcountry>
											</articles>
										</consignment>
									</shipment>";
			
			//
			// AGGIORNAMENTO DEI VALORI XML IN BASE AI PARAMETRI
			//
			XmlDocument xmlDoc = new();
			XmlDocument argsXmlDoc = new();
			xmlDoc.LoadXml(xmlString);
			argsXmlDoc.LoadXml(args);

			foreach (XmlNode arg in argsXmlDoc.SelectNodes("/data/*"))
            {
				xmlDoc.IterateThroughAllNodes(
					delegate (XmlNode node)
					{
						if (arg.Name == node.Name)
						{
							node.InnerText = arg.InnerText;
						}
					});
            }

			xmlDoc.Save(Console.Out);
			Console.WriteLine();

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