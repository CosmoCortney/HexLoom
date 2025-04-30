using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HexLoom
{
    class JsonHelpers
    {
        public static JObject SerializeProjectSettings(ProjectSettings settings)
        {
            JObject project = new JObject();
            project["ProjectName"] = settings.ProjectName;
            project["InputFilePath"] = settings.InputFilePath;
            project["OutputFilePath"] = settings.OutputFilePath;
            project["BaseAddress"] = settings.BaseAddress;
            project["IsBigEndian"] = settings.IsBigEndian;
            return project;
        }

        public static ProjectSettings DeSerializeProjectSettings(JObject settings)
        {
            ProjectSettings project = new ProjectSettings();
            project.ProjectName = settings["ProjectName"].ToString();
            project.InputFilePath = settings["InputFilePath"].ToString();
            project.OutputFilePath = settings["OutputFilePath"].ToString();
            project.BaseAddress = (UInt64)settings["BaseAddress"];
            project.IsBigEndian = (bool)settings["IsBigEndian"];
            return project;
        }

        public static JArray SerializeEntities(EntityGroup entityGroup)
        {
            var entityArr = new JArray();

            foreach (var entityS in entityGroup._EntityStack.Children)
            {
                if (entityS is not Entity)
                    return entityArr;

                var entity = entityS as Entity;
                var entityObj = new JObject();
                entityObj["EntityName"] = entity._EntityName;
                entityObj["PrimaryType"] = entity._PrimaryType;
                entityObj["SecondaryType"] = entity._SecondaryType;
                entityObj["Offset"] = entity._EntityOffset;
                entityObj["Apply"] = entity._Apply;

                if (entity._PrimaryType == (Int32)PrimaryTypes.PRIMITIVE && entity._SecondaryType == (Int32)PrimitiveTypes.BOOL)
                    entityObj["Value"] = entity._EntityValueBool;
                else
                    entityObj["Value"] = entity._EntityValue;

                entityArr.Add(entityObj);
            }

            return entityArr;
        }

        public static JArray SerializeEntityGroups(System.Collections.Generic.IList<IView> children)
        {
            var groupArr = new JArray();

            foreach (var groupS in children)
            {
                if (groupS is not EntityGroup)
                    return groupArr;

                var entityGroup = groupS as EntityGroup;
                var groupObj = new JObject();
                groupObj["GroupName"] = entityGroup._Name;
                groupObj["Collapse"] = entityGroup._Collapse;
                var entityArr = JsonHelpers.SerializeEntities(entityGroup);
                groupObj["Entities"] = entityArr;
                groupArr.Add(groupObj);
            }

            return groupArr;
        }

        public static void DeSerializeEntityGroupsToView(System.Collections.Generic.IList<IView> children, JObject settings)
        {
            children.Clear();

            foreach (Newtonsoft.Json.Linq.JObject group in settings["Groups"])
            {
                if (group == null)
                    return;

                children.Add(new EntityGroup(group));
            }
        }

        public static List<IView> DeSerializeEntityGroups(JObject settings)
        {
            List<IView> groups = new List<IView>();

            foreach (Newtonsoft.Json.Linq.JObject group in settings["Groups"])
            {
                if (group == null)
                    return groups;

                groups.Add(new EntityGroup(group));
            }

            return groups;
        }
    }
}
