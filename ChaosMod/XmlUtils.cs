﻿using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace ChaosMod
{
    static class XmlUtils
    {
        static private string baseResourceString = "Chaos_Mod.Resources.";
        static private string dataDirectory = "Data\\";

        static public XNamespace xsiNamespace
            => "http://www.w3.org/2001/XMLSchema-instance";

        static private void xmlValidationEventHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Warning)
            {
                Debug.WriteLine("WARNING: " + e.Message);
            }
            else
            {
                Debug.WriteLine("ERROR: " + e.Message);
            }
        }

        static private XmlSchemaSet getXmlSchemaSet(string filename)
        {
            var x = Assembly.GetExecutingAssembly().GetManifestResourceStream(baseResourceString + filename);

            var schemas = new XmlSchemaSet();
            schemas.Add(XmlSchema.Read(x, null));

            return schemas;
        }

        static public XmlDocument getXmlDocument(string prefix, string filename)
        {
            // TODO(Ligh): Deal with errors (file not found etc) here.

            var document = new XmlDocument();
            document.Load(dataDirectory + prefix + filename + ".xml");
            document.Schemas = getXmlSchemaSet(filename + ".xsd");
            document.Validate(new ValidationEventHandler(xmlValidationEventHandler));

            return document;
        }

        static public XDocument getXDocument(string prefix, string filename)
        {
            // TODO(Ligh): Deal with errors (file not found etc) here.

            var document = XDocument.Load(dataDirectory + prefix + filename + ".xml");
            document.Validate(getXmlSchemaSet(filename + ".xsd"), new ValidationEventHandler(xmlValidationEventHandler));

            return document;
        }
    }
}