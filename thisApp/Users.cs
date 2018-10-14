using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ReadAndVerify
{
    class Users
    {
        public string path;

        public Users(string path = "../../xml/Users.xml")
        {
            this.path = path;
        }

        /// <summary>
        /// 01 - Получить xml документ
        /// 02 - Получить элементы xml документа
        /// 03 - Получить юзера по ид 
        /// 04 - Получить юзера по ид (с документом, только в классе)
        /// 05 - Получить пароль по логину юзера
        /// 06 - Создать новый уникальный ид (только в классе)
        /// 07 - Создать нового юзера
        /// 08 - Добавить группу инструкций для пользователя по ид (с документом, только в классе)
        /// 09 - Добавить группу инструкций для пользователя по ид
        /// 10 - Обновить пароль по ид пользователя
        /// 11 - Удалить пользователя по ид
        /// 12 - Получить лист всех пользователей со всем содержимым
        /// 13 - Получить список ид прочитанных документов по ид пользователя
        /// 14 - Получить список ид не прочитанных документов по ид пользователя
        /// 15 - Переместить документ из непрочитанных в прочитанные
        /// 16 - Получить имя пользователя по ид
        /// 17 - Получить ид по логину
        /// 18 - Обновить логин по ид
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
        public XmlNode getUserById(int id)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            try
            {
                return el.SelectSingleNode("//User[@id='" + id.ToString() + "']");
            }
            catch
            {
                return null;
            }
        }//3
        private XmlNode getUserById(int id,XmlDocument doc)
        {
            XmlElement el = getElements(doc);
            try
            {
                return el.SelectSingleNode("//User[@id='" + id.ToString() + "']");
            }
            catch
            {
                return null;
            }
        }//4
        public string getPasswordByLogin(string login)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            try
            {
                return el.SelectSingleNode("User[login='"+ login +"']/password").InnerText;
            }
            catch
            {
                //MessageBox.Show("Логин не найден");
                return null;
            }
        }//5
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
            try
            {
                return orderedNumbers.Last<int>() + 1;
            }
            catch
            {
                return 1;
            }
        }//6
        public bool createUser(string username,string login,string password,int[] groups)
        {
            if (getPasswordByLogin(login) != null) return false;
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            XmlNode newUser = dc.CreateElement("User");
            XmlAttribute attr = dc.CreateAttribute("id");
            attr.Value = getNewId().ToString();
            newUser.Attributes.Append(attr);
            XmlNode newUsername = dc.CreateElement("username");
            newUsername.InnerText = username;
            XmlNode newlogin = dc.CreateElement("login");
            newlogin.InnerText = login;
            XmlNode newpassword = dc.CreateElement("password");
            newpassword.InnerText = password;
            XmlNode newreadOn = dc.CreateElement("readOn");
            XmlNode newreadOff = dc.CreateElement("readOff");
            newUser.AppendChild(newUsername);
            newUser.AppendChild(newlogin);
            newUser.AppendChild(newpassword);
            newUser.AppendChild(newreadOn);
            newUser.AppendChild(newreadOff);
            el.AppendChild(newUser);
            dc.Save(path);
            addAllInstructionsForUser(Convert.ToInt32(attr.Value),groups, dc);
            return true;
        }//7
        private void addAllInstructionsForUser(int id,int[] group, XmlDocument doc)
        {
            XmlNode el = doc.SelectSingleNode("//User[@id='" + id.ToString() + "']/readOff"); //куда добавляем
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("../../xml/Documents.xml");
            foreach (int i in group)
            {
                XmlNode groupDocuments = xmlDocument.SelectSingleNode("//group[@id='" + i.ToString() + "']");// находим группу документов

                foreach (XmlNode document in groupDocuments.ChildNodes)
                {
                    XmlNode newDoc = doc.CreateElement("document");
                    XmlAttribute attrDoc = doc.CreateAttribute("id");
                    attrDoc.Value = document.Attributes[0].Value;
                    newDoc.Attributes.Append(attrDoc);
                    el.AppendChild(newDoc);
                }                
            }
            doc.Save(path);
        }//8
        public void addAllInstructionsForUser(int id, int[] group)
        {
            XmlDocument doc = getDocement();
            XmlNode user = getUserById(id); //куда добавляем
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("../../xml/Documents.xml");
            foreach (int i in group)
            {
                XmlNode groupDocuments = xmlDocument.SelectSingleNode("//group[@id='" + i.ToString() + "']");// находим группу документов

                foreach (XmlNode document in groupDocuments.ChildNodes)
                {
                    XmlNode newDoc = doc.CreateElement("document");
                    XmlAttribute attrDoc = doc.CreateAttribute("id");
                    attrDoc.Value = document.Attributes[0].Value;
                    newDoc.Attributes.Append(attrDoc);
                    user.AppendChild(newDoc);
                }
            }
            doc.Save(path);
        }//9
        public void updatePasswordById(int id,string newPassword)
        {
            XmlDocument doc = getDocement();
            XmlNode user = getUserById(id,doc);
            user.SelectSingleNode("password").InnerText = newPassword;
            doc.Save(path);
        }//10
        public void deleteUserById(int id)
        {
            XmlDocument doc = getDocement();
            XmlNode user = getUserById(id, doc);
        }//11
        public XmlNodeList getAllUsers()
        {
            XmlDocument doc = getDocement();
            return getElements(doc).SelectNodes("User");
        }//12
        public List<int> getReadOnDocuments(int id)
        {
            XmlNode user = getUserById(id);
            List<int> result = new List<int>();
            foreach (XmlNode document in user.SelectNodes("readOn/document"))
            {
                result.Add(Convert.ToInt32(document.Attributes[0].Value));
            }
            return result;
        }//13
        public List<int> getReadOffDocuments(int id)
        {
            XmlNode user = getUserById(id);
            List<int> result = new List<int>();
            foreach (XmlNode document in user.SelectNodes("readOff/document"))
            {
                result.Add(Convert.ToInt32(document.Attributes[0].Value));
            }
            return result;
        }//14
        public void docFromReadOffToReadOn(int idUser,int idDoc)
        {
            XmlDocument dc = getDocement();
            XmlNode user = getUserById(idUser, dc);
            XmlNode doc = user.SelectSingleNode("readOff/document[@id='"+  idDoc.ToString() +"']");
            doc.ParentNode.RemoveChild(doc);
            user.SelectSingleNode("readOn").AppendChild(doc);
            dc.Save(path);
        }//15
        public string getUsernameById(int id)
        {
            XmlDocument dc = getDocement();
            return dc.SelectSingleNode("//User[@id='" + id.ToString() + "']/username").InnerText;
        }//16
        public int getIdByLogin(string login)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            try
            {
                XmlNode xl = el.SelectSingleNode("User[login='" + login + "']");
                int ssss = Convert.ToInt32(xl.SelectSingleNode("@id").Value);
                return ssss;
            }
            catch
            {
                //MessageBox.Show("Логин не найден");
                return -1;
            }
        }//17
        public void updateLoginById(int id, string newLogin)
        {
            if (getPasswordByLogin(newLogin) != null) return;
            XmlDocument doc = getDocement();
            XmlNode user = getUserById(id, doc);
            user.SelectSingleNode("login").InnerText = newLogin;
            doc.Save(path);
        }//18
        public XmlNodeList getReadOnDocuments(int id,bool d)
        {
            XmlNode user = getUserById(id);
            return user.SelectNodes("readOn/document");
            //foreach (XmlNode document in user.SelectNodes("readOn/document"))
            //{
            //    result.Add(Convert.ToInt32(document.Attributes[0].Value));
            //}
            //return result;
        }//19
        public void addDocToReadOff(int idUser,string idDoc)
        {
            //Добавляем документ в не прочитанные по id пользователя и id документа
            XmlDocument doc = getDocement();
            XmlNode newNode = doc.CreateElement("document");
            XmlAttribute newAttr = doc.CreateAttribute("id");
            newAttr.Value = idDoc;
            newNode.Attributes.Append(newAttr);
            doc.SelectSingleNode("//User[@id='" + idUser.ToString() + "']/readOff").AppendChild(newNode);
            doc.Save(path);
        }
        public XmlNodeList getReadOffDocuments(int id, bool d)
        {
            XmlNode user = getUserById(id);
            return user.SelectNodes("readOff/document");
            //foreach (XmlNode document in user.SelectNodes("readOn/document"))
            //{
            //    result.Add(Convert.ToInt32(document.Attributes[0].Value));
            //}
            //return result;
        }//19
        public void deleteDoc(int idU,int idD)
        {
            XmlDocument dc = getDocement();
            XmlElement el = getElements(dc);
            XmlNode user = el.SelectSingleNode("//User[@id='" + idU.ToString() + "']");

            XmlNodeList readOn = user.SelectNodes("readOn/document");

            XmlNodeList readOff = user.SelectNodes("readOff/document");



            //XmlNodeList readOff = getReadOffDocuments(idU, true);
            //XmlNodeList readOn = getReadOnDocuments(idU, true);
            //XmlNode user = getUserById(idU);

            foreach (XmlNode rOff in readOff)
            {

                if (rOff.Attributes["id"].Value == idD.ToString())
                {
                    rOff.ParentNode.RemoveChild(rOff);
                    //idD.MessageBox.Show("нашел");
                }
            }
            foreach (XmlNode rOn in readOn)
            {

                if (rOn.Attributes["id"].Value == idD.ToString())
                {
                    rOn.ParentNode.RemoveChild(rOn);
                    //idD.MessageBox.Show("нашел");
                }
            }
            dc.Save(path);
        }
    }
}
