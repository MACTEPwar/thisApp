using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ReadAndVerify
{
    class LowNews
    {
        public int id;
        public string path;
        public string title;
        public string content;
        public string date;



        public LowNews(string p = @"../../lowNews.xml")
        {
            path = p;
        }

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
        }

        private XmlElement getElements(XmlDocument doc)
        {
            return doc.DocumentElement;
        }

        public LowNews(string t, string c, string d, string p = @"../../lowNews.xml")
        {
            path = p;
            title = t;
            content = c;
            date = d;
            //id = i;
            XmlDocument dc = getDocement();
            XmlNode news = dc.CreateElement("News");
            dc.DocumentElement.AppendChild(news);
            XmlNode tit = dc.CreateElement("title");
            tit.InnerText = t;
            XmlNode con = dc.CreateElement("content");
            con.InnerText = c;
            XmlNode dat = dc.CreateElement("date");
            dat.InnerText = d;
            XmlAttribute attr = dc.CreateAttribute("id");
            //attr.Value = i.ToString();
            attr.Value = getNewId().ToString();
            news.Attributes.Append(attr);
            news.AppendChild(tit);
            news.AppendChild(con);
            news.AppendChild(dat);
            dc.Save(p);
        }

        private int getNewId() // Проверить
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            List<int> val = new List<int>();
            foreach (XmlNode x in el)
            {
                val.Add(Convert.ToInt32(x.Attributes[0].Value));
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
        }

        public List<LowNews> getAllNews()
        {
            List<LowNews> lwList = new List<LowNews>();
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            //dc = new XmlDocument();
            dc.Load(path);
            el = dc.DocumentElement;
            foreach(XmlNode child in el)
            {
                LowNews lw = new LowNews(path);
                lw.id = Convert.ToInt32(child.Attributes[0].Value);
                foreach (XmlNode subChild in child)
                {
                    switch(subChild.Name)
                    {
                        case "title":
                            {
                                lw.title = subChild.InnerText;
                                break;
                            }
                        case "content":
                            {
                                lw.content = subChild.InnerText;
                                break;
                            }
                        case "date":
                            {
                                lw.date = subChild.InnerText;
                                break;
                            }
                    }
                }
                lwList.Add(lw);
            }
            return lwList;
        }

        public List<LowNews> getSortedNews(string state)
        {
            List<LowNews> list = getAllNews();
            List<LowNews> result = new List<LowNews>();
            switch (state)
            {
                case "ASC":
                    {
                        LowNews temp = new LowNews(path);
                        foreach (LowNews ln in list)
                        {
                            if (ln == list.First<LowNews>())
                            {
                                temp = ln;
                                result.Add(ln);
                                continue;
                            }
                            if (Convert.ToDateTime(ln.date).CompareTo(Convert.ToDateTime(temp.date)) != -1)
                            {
                                result.Insert(0, ln);
                                temp = result.First<LowNews>();
                                continue;
                            }
                            else if (Convert.ToDateTime(ln.date).CompareTo(Convert.ToDateTime(temp.date)) == -1)
                            {
                                int ind = 0;
                                bool exi = false;
                                foreach (LowNews lwRes in result)
                                {
                                    if (Convert.ToDateTime(ln.date).CompareTo(Convert.ToDateTime(lwRes.date)) != -1)
                                    {
                                        result.Insert(ind, ln);
                                        exi = true;
                                        break;
                                    }

                                    ind++;
                                }
                                if (!exi)
                                {
                                    result.Add(ln);
                                }
                                temp = result.First<LowNews>();
                                continue;
                            }
                        }
                        result.Reverse();
                        return result;
                    }
                case "DESC":
                    {
                        LowNews temp = new LowNews(path);
                        foreach (LowNews ln in list)
                        {
                            if (ln == list.First<LowNews>())
                            {
                                temp = ln;
                                result.Add(ln);
                                continue;
                            }
                            if (Convert.ToDateTime(ln.date).CompareTo(Convert.ToDateTime(temp.date)) != -1)
                            {
                                result.Insert(0, ln);
                                temp = result.First<LowNews>();
                                continue;
                            }
                            else if (Convert.ToDateTime(ln.date).CompareTo(Convert.ToDateTime(temp.date)) == -1)
                            {
                                int ind = 0;
                                bool exi = false;
                                foreach (LowNews lwRes in result)
                                {
                                    if (Convert.ToDateTime(ln.date).CompareTo(Convert.ToDateTime(lwRes.date)) != -1)
                                    {
                                        result.Insert(ind, ln);
                                        exi = true;
                                        break;
                                    }
                                    
                                    ind++;
                                }
                                if (!exi)
                                {
                                    result.Add(ln);
                                }
                                temp = result.First<LowNews>();
                                continue;
                            }
                        }
                        return result;
                    }
            }
            return result;
        }

        private XmlNode findNews(int id, XmlDocument ds)
        {
            //ds = getDocement();
            XmlElement el = getElements(ds);
            try
            {
                return el.SelectSingleNode("//News[@id='" + id.ToString() + "']");
            }
            catch
            {
                MessageBox.Show("Новость ен найдена по указанному id.");
                return null;
            }
        }

        public void updateNews(int id, string title,string content,string date)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            XmlNode curNews = findNews(id,dc);
           
            foreach (XmlNode child in curNews.ChildNodes)
            {
                switch (child.Name)
                {
                    case "title":
                        {
                            child.InnerText = title;
                            break;
                        }
                    case "content":
                        {
                            child.InnerText = content;
                            break;
                        }
                    case "date":
                        {
                            child.InnerText = date;
                            break;
                        }
                }
                MessageBox.Show(child.Name + "\n" + child.InnerText);
            }
            dc.Save(path);
        }

        public void deleteNews(int id)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            try
            {
                XmlNode n = el.SelectSingleNode("//News[@id='" + id.ToString() + "']");
                n.ParentNode.RemoveChild(n);
                dc.Save(path);
            }
            catch
            {
                MessageBox.Show("Не верно указан id объекта для удаления.");
            }
        }
    }
}
