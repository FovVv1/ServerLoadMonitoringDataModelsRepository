using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NLog;
using ServerLoadMonitoringDataModels;

namespace ServerLoadMonitoringDataModels {
	[Serializable()]
	[DataContract]
	public class AgentConfig {
		public string Name { get; set; }
		
		[DataMember]
		public List<Job> Jobs { get; set; }


      /// <summary>
      /// Сохранение конфигурации в формате XML
      /// </summary>
      /// <returns></returns>
      public bool SaveToXml(string path) {
         try {


            XmlDocument doc = new XmlDocument();

            doc.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?><Jobs></Jobs>");
            var xRoot = doc.DocumentElement;

            foreach (var el in Jobs) {
               // создаем новый элемент работы
               var job = doc.CreateElement("Job");

               //регестрируем имя работы
               var nameNode = doc.CreateElement("Name");
               var nameText = doc.CreateTextNode(el.Name);
               nameNode.AppendChild(nameText);
               job.AppendChild(nameNode);

               //регестриуем имя библиотеки для выполнения работы
               var dllNode = doc.CreateElement("Dll");
               var dllText = doc.CreateTextNode($"{el.DLL}");
               dllNode.AppendChild(dllText);
               job.AppendChild(dllNode);

               //регестриуем описание  работы
               var descriptionNode = doc.CreateElement("Description");
               var descriptionText = doc.CreateTextNode($"{el.Description}");
               descriptionNode.AppendChild(descriptionText);
               job.AppendChild(descriptionNode);

               //регестриуем описание  работы
               var scheduleTimeNode = doc.CreateElement("ScheduleTime");
               var scheduleTimeText = doc.CreateTextNode($"{el.ScheduleTime}");
               scheduleTimeNode.AppendChild(scheduleTimeText);
               job.AppendChild(scheduleTimeNode);

               //регестриуем узел свойств
               var propertiesNode = doc.CreateElement("Properties");

               foreach (var property in el.Properties) {
                  //регестриуем описание  работы
                  var propertyNode = doc.CreateElement("Property");
                  // в атрибуте регестрируем ключ славаря
                  var nameAttr = doc.CreateAttribute($"{property.Key}");
                  //в текст регестрируем значение 
                  var valueText = doc.CreateTextNode($"{property.Value}");
                  nameAttr.AppendChild(valueText);

                  propertyNode.Attributes.Append(nameAttr);

                  propertiesNode.AppendChild(propertyNode);
               }
               job.AppendChild(propertiesNode);

               //регестриуем узел с путями файлов
               var filesPath = doc.CreateElement("AllFilesPath");

               foreach (var filePath in el.AllFilesPath) {
                  //регестриуем описание  работы
                  var filePathNode = doc.CreateElement("Path");
                  var valueText = doc.CreateTextNode($"{filePath}");
                  filePathNode.AppendChild(valueText);
                  filesPath.AppendChild(filePathNode);
               }
               job.AppendChild(filesPath);

               //регестриуем узел с путями директорий
               var allDirectoryPath = doc.CreateElement("AllDirectoryPath");

               foreach (var directoryPath in el.AllDirectoryPath) {
                  //регестриуем описание  работы
                  var directoryPathNode = doc.CreateElement("Path");
                  var valueText = doc.CreateTextNode($"{directoryPath}");
                  directoryPathNode.AppendChild(valueText);
                  allDirectoryPath.AppendChild(directoryPathNode);
               }

               job.AppendChild(allDirectoryPath);


               job.AppendChild(nameNode);
               xRoot?.AppendChild(job);
            }




            doc.Save(path);

            return true;
         } catch (Exception e) {
            LogManager.GetCurrentClassLogger().Error($"Ошибка записи файла конфигурации {path}. {e.Message}");
            return false;
         }


      }

      /// <summary>
      /// Сохранение конфигурации в формате XML
      /// </summary>
      /// <returns></returns>
      public bool LoadToXml(string path) {
         try {
            var xDoc = new XmlDocument();
            xDoc.Load(path);
            // получим корневой элемент
            var xRoot = xDoc.DocumentElement;
            if (xRoot == null)
               return true;
            Jobs = new List<Job>();
            // обход всех узлов в корневом элементе
            foreach (XmlElement xnode in xRoot) {
               if (xnode.Name != "Job")
                  continue;
               var tmpJob = new Job();

               //// обходим все дочерние узлы элемента user
               foreach (XmlNode childnode in xnode.ChildNodes) {
                  if (childnode.Name == "Name")
                     tmpJob.Name = childnode.InnerText;
                  if (childnode.Name == "Dll")
                     tmpJob.DLL = childnode.InnerText;
                  if (childnode.Name == "Description")
                     tmpJob.Description = childnode.InnerText;
                  if (childnode.Name == "ScheduleTime")
                     tmpJob.ScheduleTime = int.Parse(childnode.InnerText);
                  //Чтение словоря свойств работы
                  if (childnode.Name == "Properties") {
                     tmpJob.Properties = new Dictionary<string, string>();
                     foreach (XmlNode property in childnode.ChildNodes) {
                        if (property.Attributes is null)
                           continue;
                        foreach (var attr in property.Attributes) {
                           if (attr is XmlAttribute attribute) {
                              tmpJob.Properties.Add(attribute.Name, attribute.Value);
                           }

                        }

                     }
                  }

                  //Чтение спсика путей к дтректориям бэкапа
                  if (childnode.Name == "AllDirectoryPath") {
                     tmpJob.AllDirectoryPath = new List<string>();
                     foreach (XmlNode directoryPath in childnode.ChildNodes) {
                        tmpJob.AllDirectoryPath.Add(directoryPath.InnerText);
                     }
                  }

                  //Чтение списка путей к файлам бэкапа
                  if (childnode.Name == "AllFilesPath") {
                     tmpJob.AllFilesPath = new List<string>();
                     foreach (XmlNode filesPath in childnode.ChildNodes) {
                        tmpJob.AllFilesPath.Add(filesPath.InnerText);
                     }
                  }




               }

               Jobs.Add(tmpJob);
            }

            return true;
         } catch (Exception e) {
            LogManager.GetCurrentClassLogger().Error($"Ошибка чтения файла конфигурации {path}. {e.Message}");
            return false;
         }


      }
   }
}
