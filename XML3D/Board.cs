using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace XML3D
{
    class Board
    {
        public List<Component> components;
        public List<Circle> circles;
        //public List<Point> point;
        public List<Object> sketh, cutout;
        public double thickness;
        public int ver;

        public static Board GetfromXML(string filename)
        {
            Board board = new Board();
            XDocument doc=GetXML(filename, board);
            if (doc == null) { return null; }
            IEnumerable<XElement> elements = doc.Root.Elements();
            board.components = new List<Component>();
            foreach (XElement e in elements)
            {
                if (e.FirstAttribute.Name == "AD_ID") { GetBodyfromXElement(e, board); }
                if (e.FirstAttribute.Name == "ID") { board.components.Add(GetfromXElement(e)); }
            }
            return board;
        }
        private static XDocument GetXML(string filename, Board board)
        {
            XDocument doc = XDocument.Load(filename);
            XElement element = (XElement)doc.Root.FirstNode;
            switch (element.Name.ToString())
            {
                case "transactions":
                    board.ver = 1;
                    return GetXML1(filename);   
                case "transaction":
                     if (element.Attribute("Type").Value == "SOLIDWORKS") 
                    {
                        board.ver = 3; 
                        return null;// GetXML3(filename); 
                    }
                    else
                    {
                        board.ver = 2;
                        return GetXML2(filename);
                    }
                default:
                    return null;
            }
         }
        private static XDocument GetXML1(string filename)
        {
            XAttribute a1, a2;
            XDocument doc_out, doc = XDocument.Load(filename);
            XElement componentXML, atribute, XML;
            IEnumerable<XElement> elements2, elements1;
            doc_out = new XDocument();
            XML = new XElement("XML");
            doc_out.Add(XML);
            try
            {
                elements1 = doc.Root.Element("transactions").Element("transaction").Element("document").Element("configuration").Element("references").Elements();
              
                foreach (XElement e1 in elements1)
                {
                    elements2 = e1.Element("configuration").Elements();
                    if (e1.FirstAttribute.Value == "Документация") { continue; }
                    componentXML = new XElement("componentXML", new XAttribute(e1.FirstAttribute.Name, e1.FirstAttribute.Value));

                    foreach (XElement e2 in elements2)
                    {
                        a1 = e2.Attribute("name");
                        a2 = e2.Attribute("value");
                        if (a1.Value == "Раздел_Сп" | a1.Value == "Fitted" | a1.Value == "GUID") { continue; }
                        atribute = new XElement("attribute", a1, a2);
                        componentXML.Add(atribute);
                    }
                    XML.Add(componentXML);
                }
            }
            catch
            {
                return null;
            }
          
         return doc_out;
            
        }
        private static XDocument GetXML2(string filename)
        {
            string descriptionPCB = "";
            string comnpName = "";
            XAttribute a1, a2;
            XDocument doc_out, doc = XDocument.Load(filename);
            XElement componentXML, atribute, XML, tmpXEl;
            IEnumerable<XElement> elements, elements2;
            doc_out = new XDocument();
            XML = new XElement("XML");
            doc_out.Add(XML);
            try
            {
                elements = doc.Root.Element("transaction").Element("project").Element("configurations").Element("configuration").Element("graphs").Elements();
                tmpXEl = elements.First(item => item.Attribute("name").Value.Equals("Обозначение_PCB"));
                descriptionPCB = tmpXEl.Attribute("value").Value;
                componentXML = new XElement("componentXML", new XAttribute("AD_ID", descriptionPCB));
                elements = doc.Root.Element("transaction").Element("project").Element("configurations").Element("configuration").Element("componentsPCB").Element("component_pcb").Element("properties").Elements();
                foreach (XElement e in elements)
                {
                    a1 = e.Attribute("name");
                    a2 = e.Attribute("value");
                    //if (a1.Value == "Раздел_Сп" | a1.Value == "Fitted" | a1.Value == "GUID") { continue; }
                    atribute = new XElement("attribute", a1, a2);
                    componentXML.Add(atribute);
                }
                XML.Add(componentXML);
                elements = doc.Root.Element("transaction").Element("project").Element("configurations").Element("configuration").Element("components").Elements();
                foreach (XElement e in elements)
                {
                    elements2 = e.Element("properties").Elements();
                    tmpXEl = elements2.First(item => item.Attribute("name").Value.Equals("Наименование"));
                    comnpName = tmpXEl.Attribute("value").Value;
                    componentXML = new XElement("componentXML", new XAttribute("ID", comnpName));
                    foreach (XElement e2 in elements2)
                    {
                        atribute = new XElement("attribute", e2.Attribute("name"), e2.Attribute("value"));
                        componentXML.Add(atribute);
                    }
                    XML.Add(componentXML);
                }
            }
            catch
            {
                return null;
            }
            return doc_out;
        }
              private static Component GetfromXElement(XElement el)
        {
            IEnumerable<XElement> elements = el.Elements();
            Component component = new Component();
            foreach (XElement e in elements)
            {
                switch (e.Attribute("name").Value)
                {
                    case "DM_PhysicalDesignator":
                        component.physicalDesignator = e.Attribute("value").Value;
                        break;
                    case "Позиционное обозначение":
                        component.physicalDesignator = e.Attribute("value").Value;
                        break;
                    case "Наименование":
                        component.title = e.Attribute("value").Value;
                        break;
                    case "Footprint":
                        component.footprint = e.Attribute("value").Value;
                        break;
                    case "Part Number":
                        component.part_Number = e.Attribute("value").Value;
                        break;
                    case "X":
                        component.x = double.Parse(e.Attribute("value").Value) / 1000;
                        break;
                    case "Y":
                        component.y = double.Parse(e.Attribute("value").Value) / 1000;
                        break;
                    case "Z":
                        component.z = double.Parse(e.Attribute("value").Value) / 1000;
                        break;
                    case "Layer":
                        component.layer = int.Parse(e.Attribute("value").Value);
                        break;
                    case "Rotation":
                        component.rotation = double.Parse(e.Attribute("value").Value);
                        break;
                    case "StandOff":
                        component.standOff = double.Parse(e.Attribute("value").Value) / 1000;
                        break;
                }
            }
            return component;
        }
        private static void GetBodyfromXElement(XElement el, Board board)
        {
            IEnumerable<XElement> elements = el.Elements();
            string str;
            string[] strsplit, strToDbl;
            Line skLine;
            Arc skArc;
            Circle skCircle;
            board.sketh = new List<object>();
            board.cutout = new List<object>();
            board.circles = new List<Circle>();
            foreach (XElement e in elements)
            {
                switch (e.Attribute("name").Value)
                {
                    case "BOARD_OUTLINE":
                        str = e.Attribute("value").Value;
                        strsplit = str.Split((char)35);
                        for (int i = 0; i < strsplit.Length; i++)
                        {
                            strToDbl = strsplit[i].Split((char)59);
                            if (strToDbl.Length == 4)
                            {
                                skLine = new Line();
                                skLine.x1 = double.Parse(strToDbl[0]) / 1000;
                                skLine.y1 = double.Parse(strToDbl[1]) / 1000;
                                skLine.x2 = double.Parse(strToDbl[2]) / 1000;
                                skLine.y2 = double.Parse(strToDbl[3]) / 1000;
                                board.sketh.Add(skLine);
                            }
                            if (strToDbl.Length == 9)
                            {
                                skArc = new Arc();
                                skArc.x1 = double.Parse(strToDbl[0]) / 1000;
                                skArc.y1 = double.Parse(strToDbl[1]) / 1000;
                                skArc.x2 = double.Parse(strToDbl[2]) / 1000;
                                skArc.y2 = double.Parse(strToDbl[3]) / 1000;
                                skArc.xc = double.Parse(strToDbl[7]) / 1000;
                                skArc.yc = double.Parse(strToDbl[8]) / 1000;
                                skArc.direction = 0;
                                board.sketh.Add(skArc);
                            }
                        }
                        break;
                    case "BOARD_CUTOUT":
                        str = e.Attribute("value").Value;
                        strsplit = str.Split((char)35);
                        for (int i = 0; i < strsplit.Length; i++)
                        {
                            strToDbl = strsplit[i].Split((char)59);
                            if (strToDbl.Length == 4)
                            {
                                skLine = new Line();
                                skLine.x1 = double.Parse(strToDbl[0]) / 1000;
                                skLine.y1 = double.Parse(strToDbl[1]) / 1000;
                                skLine.x2 = double.Parse(strToDbl[2]) / 1000;
                                skLine.y2 = double.Parse(strToDbl[3]) / 1000;
                                board.cutout.Add(skLine);
                            }
                            if (strToDbl.Length == 9)
                            {
                                skArc = new Arc();
                                skArc.x1 = double.Parse(strToDbl[0]) / 1000;
                                skArc.y1 = double.Parse(strToDbl[1]) / 1000;
                                skArc.x2 = double.Parse(strToDbl[2]) / 1000;
                                skArc.y2 = double.Parse(strToDbl[3]) / 1000;
                                skArc.xc = double.Parse(strToDbl[7]) / 1000;
                                skArc.yc = double.Parse(strToDbl[8]) / 1000;
                                skArc.direction = 1;
                                board.cutout.Add(skArc);
                            }
                        }
                        break;
                    case "DRILLED_HOLES":
                        str = e.Attribute("value").Value;
                        strsplit = str.Split((char)35);
                        for (int i = 0; i < strsplit.Length; i++)
                        {
                            strToDbl = strsplit[i].Split((char)59);
                            if (strToDbl.Length == 4)
                            {
                                skCircle = new Circle();
                                skCircle.radius = double.Parse(strToDbl[0]) / 2000;
                                skCircle.xc = double.Parse(strToDbl[1]) / 1000;
                                skCircle.yc = double.Parse(strToDbl[2]) / 1000;
                                board.circles.Add(skCircle);
                            }
                        }
                        break;
                    case "Толщина, мм":
                        board.thickness = double.Parse(e.Attribute("value").Value) / 1000;
                        break;
                }
            }
        }    
    }
}
