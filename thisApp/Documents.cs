using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ReadAndVerify
{
    class Documents
    {
        public string path;

        public Documents(string path = "../../xml/Documents.xml")
        {
            this.path = path;
            docX = getDocement();
            elementX = getElements(docX);
        }

        XmlDocument docX = null;
        XmlElement elementX;
        /// <summary>
        /// 01 - Получить xml документ
        /// 02 - Получить элементы xml документа
        /// 03 - Получить документ по его ид
        /// 04 - Создать группу. False если ошибка
        /// 05 - Удалить группу по названию. False если ошибка
        /// 06 - Изменить название группы по ид. False если ошибка
        /// 07 - Получить новый, уникальный ид для группы
        /// 08 - Получить ид группы по названию. -1 если ошибка
        /// 09 - Получить все документы группы по ее ид. null если ошибка
        /// 10 - Проверить существования группы по названию
        /// 11 - Удалить группу по ид. False если ошибка
        /// 12 - Заполнить все файлы с папки
        /// 13 - Создать документ
        /// 14 - Получить новый, уникальный ид для документа
        /// 15 - Переместить документ(ид) в группу(название)
        /// </summary>
        /// <returns></returns>
        private XmlDocument getDocement()
        {
            XmlDocument dc = new XmlDocument();
            try
            {
                dc.Load(path);
            }
            catch
            {
                MessageBox.Show("Файл " + path + "не существует.");
            }
            return dc;
        }//1
        private XmlElement getElements(XmlDocument doc)
        {
            return doc.DocumentElement;
        }//2
        public XmlNode getDocumentById(int id)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            //XmlNode tst1 = el.SelectSingleNode("group");
            //XmlNode tst2 = el.SelectSingleNode("group/document[@id='6']");
            XmlNode docThis = el.SelectSingleNode("group/document[@id='" + id.ToString() + "']");
            return docThis;
        }//3
        public bool createGroup(string title)
        {
            if (new Documents().isExist(title)) return false;
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            XmlNode newGroup = dc.CreateElement("group");
            XmlAttribute attr1 = dc.CreateAttribute("id");
            attr1.Value = getNewId().ToString();
            newGroup.Attributes.Append(attr1);
            XmlAttribute attr2 = dc.CreateAttribute("name");
            attr2.Value = title;
            newGroup.Attributes.Append(attr2);
            el.AppendChild(newGroup);
            dc.Save(path);
            return true;
        }//4
        public bool deleteGroup(string title)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            try
            {
                XmlNode item = el.SelectSingleNode("group[@name='" + title + "']");
                item.ParentNode.RemoveChild(item);
                dc.Save(path);
                return true;
            }
            catch
            {
                return false;
            }
        }//5
        public bool updateGroup(int id,string title)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            try
            {
                XmlNode item = el.SelectSingleNode("group[@id='" + id.ToString() + "']");
                item.Attributes["name"].Value = title;
                dc.Save(path);
                return true;
            }
            catch
            {
                return false;
            }
        }//6
        private int getNewId()
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            List<int> val = new List<int>();
            foreach (XmlNode x in el.ChildNodes)
            {
                val.Add(Convert.ToInt32(x.Attributes[0].Value));
            }
            var orderedNumbers = from i in val
                                 orderby i
                                 select i;
            return orderedNumbers.Last<int>() + 1;
        }//7
        public int getIdGroupByTitle(string title)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            if (isExist(title))
                //MessageBox.Show(el.SelectSingleNode("group[@name='" + title + "']").Attributes["id"].Value.ToString());
                 return Convert.ToInt32(el.SelectSingleNode("group[@name='" + title + "']").Attributes["id"].Value);
            return -1;
        }//8
        public XmlNodeList getDocumentsByGroupId(int id)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            try
            {
                return el.SelectNodes("group[@id='"+ id.ToString() +"']/document");
            }
            catch
            {
                return null;
            }
        }//9
        public bool isExist(string title)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            foreach(XmlNode item in el)
            {
                if (item.Attributes["name"].Value == title) return true;
            }
            return false;
        }//10
        public bool deleteGroup(int id)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            try
            {
                XmlNode item = el.SelectSingleNode("group[@id='" + id.ToString() + "']");
                item.ParentNode.RemoveChild(item);
                dc.Save(path);
                return true;
            }
            catch
            {
                return false;
            }
        }//11
        public void fillStandartGroup()
        {
            
            if (Directory.Exists(Properties.Settings.Default.workDir))
            {
                if (!new Documents().isExist("all")) createGroup("all");
                XmlDocument dc = getDocement();
                XmlElement el = getElements(dc);
                XmlNode all = el.SelectSingleNode("group[@name='all']");
                XmlNodeList _all = getDocumentsByGroupId(getIdGroupByTitle("all"));
                //all.RemoveAll();
                //dc.Save(path);
                FileInfo[] files = new DirectoryInfo(Properties.Settings.Default.workDir).GetFiles();
                foreach(FileInfo file in files)
                {
                    bool _fileIsExsist = false;
                    foreach(XmlNode thisAll in _all)
                    {
                        if (file.Name == thisAll.SelectSingleNode("@name").Value)
                        {
                            _fileIsExsist = true;
                        }
                    }
                    if (!_fileIsExsist)
                    {
                        createDocument(file.Name, file.FullName, all, dc);
                        dc.Save(path);
                    }
                    _fileIsExsist = false;
                }
                all.Attributes.Append(dc.CreateAttribute("id")).Value = "1";
                all.Attributes.Append(dc.CreateAttribute("name")).Value = "all";
                dc.Save(path);
            }
        }//12
        public void createDocument(string title,string pathD,XmlNode parent, XmlDocument dc)
        {
            XmlElement el = getElements(dc);
            XmlNode newDoc = dc.CreateElement("document");
            newDoc.Attributes.Append(dc.CreateAttribute("id")).Value = getNewIdForDoc().ToString();
            newDoc.Attributes.Append(dc.CreateAttribute("name")).Value = title;
            newDoc.Attributes.Append(dc.CreateAttribute("path")).Value = pathD;
            parent.AppendChild(newDoc);
            dc.Save(path);
        }//13
        private int getNewIdForDoc()
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            List<int> val = new List<int>();
            foreach (XmlNode x in el.ChildNodes)
            {
                foreach (XmlNode xx in x.ChildNodes)
                {
                    val.Add(Convert.ToInt32(xx.Attributes["id"].Value));
                }
                //val.Add(Convert.ToInt32(x.Attributes[0].Value));
            }
            var orderedNumbers = from i in val
                                 orderby i
                                 select i;
            try
            {
                return orderedNumbers.Last<int>() + 1;
            }
            catch
            {
                return 1;
            }
        }//14
        public void replaceDocForGroup(int idDoc,string titleGr)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            XmlNode doc = el.SelectSingleNode("group/document[@id='" + idDoc + "']");
            XmlNode gr = el.SelectSingleNode("group[@name='" + titleGr + "']");
            gr.AppendChild(doc);
            if (doc.ParentNode!= gr) doc.ParentNode.RemoveChild(doc);
            dc.Save(path);
        }//15
        public XmlElement getAllGroups()
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            return el;
        }
        public bool deleteDocument(int id)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            try
            {
                XmlNode item = el.SelectSingleNode("group/document[@id='" + id.ToString() + "']");
                item.ParentNode.RemoveChild(item);
                dc.Save(path);
                return true;
            }
            catch
            {
                return false;
            }
        }//
        public void deleatExistingItems(int id)
        {
            XmlNodeList readOnDoc = new Users().getReadOnDocuments(id, true);
            XmlNodeList readOffDoc = new Users().getReadOffDocuments(id, true);
            var List = readOnDoc.Cast<XmlNode>().Concat<XmlNode>(readOffDoc.Cast<XmlNode>());
            FileInfo[] fiAll = new DirectoryInfo(Properties.Settings.Default.workDir).GetFiles();
            foreach (XmlNode doc in List) //foreach (FileInfo fi in fiAll)
            {
                bool fileEx = false;
                foreach (FileInfo fi in fiAll) //foreach (XmlNode doc in List)
                {
                    int idT = Convert.ToInt32(doc.SelectSingleNode("@id").Value);
                    XmlNode docT = new Documents().getDocumentById(idT);
                    string name = string.Empty;
                    try
                    {
                        name = docT.SelectSingleNode("@name").Value;
                    }
                    catch
                    {
                        new Users().deleteDoc(id, idT);
                    }
                    if (name == fi.Name)
                    {
                        fileEx = true;
                        break;
                    }
                }
                if (!fileEx)
                {
                    new Documents().deleteDocument(Convert.ToInt32(doc.SelectSingleNode("@id").Value));
                }
                fileEx = false;
            }
        }
    }
}
