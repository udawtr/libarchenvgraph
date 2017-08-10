using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    using Modules;
    using System.Reflection;

    /// <summary>
    /// 計算グラフの分析を行うクラス
    /// </summary>
    public static class GraphAnalyzer
    {
        #region モジュール階層の出力

        public static void TraceNestedModule(Modules.ContainerModule rootContainer)
        {
            WriteModuleInfo(rootContainer, 0);

            InternalTraceNestedModule(rootContainer, 0);
        }

        private static void WriteModuleInfo(ICalculationGraph module, int indentLevel)
        {
            char[] indentC = new char[indentLevel];
            if (indentLevel > 0)
            {
                indentC[0] = '+';
                for (int i = 1; i < indentLevel; i++)
                {
                    indentC[i] = '-';
                }
            }
            System.Diagnostics.Debug.WriteLine($"{new String(indentC)}{module}");
        }

        private static void InternalTraceNestedModule(ContainerModule rootContainer, int indentLevel)
        {
            foreach (var item in rootContainer.Modules)
            {
                WriteModuleInfo(item, indentLevel+2);

                if (item is Modules.ContainerModule)
                {
                    InternalTraceNestedModule(item as Modules.ContainerModule, indentLevel+2);
                }
            }
        }

        #endregion

        public static List<ICalculationGraph> GetAllModules(ContainerModule container)
        {
            var list = new List<ICalculationGraph>();

            list.Add(container);

            InnerGetAllModules(container, list);

            return list;
        }

        private static void InnerGetAllModules(ContainerModule container, List<ICalculationGraph> list)
        {
            foreach (var item in container.Modules)
            {
                list.Add(item);

                if (item is Modules.ContainerModule)
                {
                    InnerGetAllModules(item as ContainerModule, list);
                }
            }
        }

        public static void CheckVariableLoop(object varContainer)
        {
            //対象モジュールのVariableを列挙
            var varList = GetVariableList(varContainer);

            //それぞれのVariableに対して参照を辿る
            foreach (var varItem in varList)
            {
                //LinkVariableから出発する場合のみ確認
                if (IsLinkVariable(varItem.PropertyType))
                {
                    //プロパティ間での相互参照チェック
                    var varValue = varItem.GetValue(varContainer);
                    var refProp = varList.Where(x => varItem != x).SingleOrDefault(x => x.GetValue(varContainer) == varValue);
                    if (refProp != null)
                    {
                        throw new Exception($"{varValue}({varItem.Name})の{varItem.Name}が{varContainer}の{refProp.Name}を参照しています。");
                    }

                    InnerCheckVariableLoop(varContainer, varItem, varContainer, varList);
                }
            }
        }

        private static void InnerCheckVariableLoop(object varItemContainer, PropertyInfo varItem, object varRootContainer, List<PropertyInfo> varList)
        {
            //プロパティを取得
            var varValue = varItem.GetValue(varItemContainer);
            if (varValue != null)
            {
                if (varValue == varRootContainer) throw new Exception($"'{varRootContainer}'の{varItem.Name}が'{varRootContainer}'を参照しています。");

                var varList2 = GetVariableList(varValue);
                foreach (var varItem2 in varList2)
                {
                    var varValue2 = varItem2.GetValue(varValue);
                    var refProp2 = varList.SingleOrDefault(x => x.GetValue(varRootContainer) == varValue2);
                    if (refProp2 != null)
                    {
                        throw new Exception($"{varValue}({varItem.Name})の{varItem2.Name}が{varRootContainer}の{refProp2.Name}を参照しています。");
                    }

                    InnerCheckVariableLoop(varValue, varItem2, varRootContainer, varList);
                }
            }
        }

        public static List<System.Reflection.PropertyInfo> GetVariableList(object varContainer)
        {
            var list = new List<System.Reflection.PropertyInfo>();

            var T = varContainer.GetType();
            var properties = T.GetProperties();

            foreach (var prop in properties)
            {
                if(IsIVariable(prop.PropertyType))
                { 
                    list.Add(prop);
                }
            }

            return list;
        }

        private static bool IsIVariable(Type type)
        {
            foreach (var propT in GetParentTypes(type))
            {
                if (propT.IsGenericType && propT.GetGenericTypeDefinition() == typeof(IVariable<>))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsLinkVariable(Type type)
        {
            foreach (var propT in GetParentTypes(type))
            {
                if (propT.IsGenericType && propT.GetGenericTypeDefinition() == typeof(LinkVariable<>))
                {
                    return true;
                }
            }
            return false;
        }

        private static IEnumerable<Type> GetParentTypes(this Type type)
        {
            //contain self type
            yield return type;

            // is there any base type?
            if ((type == null) || (type.BaseType == null))
            {
                yield break;
            }

            // return all implemented or inherited interfaces
            foreach (var i in type.GetInterfaces())
            {
                yield return i;
            }

            // return all inherited types
            var currentBaseType = type.BaseType;
            while (currentBaseType != null)
            {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }
    }
}
