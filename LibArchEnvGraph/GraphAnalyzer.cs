using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibArchEnvGraph
{
    using Modules;
    using System.Collections;
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
                //LinkVariableから出発する場合は無視
                if (IsLinkVariable(varItem.PropertyType) || IsLinkVariable(GetEnumeratedType(varItem.PropertyType)))
                {
                    continue;
                }

                System.Diagnostics.Debug.Write($"* {varContainer}.{varItem.Name}");

                //プロパティ間での相互参照チェック
                var varValue = varItem.GetValue(varContainer);

                // 値がnullの場合は辿れません。
                if (varValue == null)
                {
                    System.Diagnostics.Debug.WriteLine($" -> null");
                    continue;
                }

                //出力の参照が入力に設定されていないか?
                PropertyInfo refProp = varList.Where(x => varItem != x).SingleOrDefault(x => x.GetValue(varContainer) == varValue);
                if (refProp != null 
                && (IsLinkVariable(refProp.PropertyType) || IsLinkVariable(GetEnumeratedType(refProp.PropertyType))) )
                {
                    throw new Exception($"{varValue}({varItem.Name})の{varItem.Name}が{varContainer}の{refProp.Name}を参照しています。");
                }

                System.Diagnostics.Debug.WriteLine("");
                InnerCheckVariableLoop(varContainer, varItem, varContainer, varList);
            }
        }

        private static void InnerCheckVariableLoop(object containerInstance, PropertyInfo inVariableProperty, object varRootContainer, List<PropertyInfo> varList)
        {
            //モジュールの入力変数を展開して確認
            foreach(object inVariableInstance in ExpandValues(inVariableProperty, containerInstance))
            {
                //
                //ファンクションの入力がファンクション自体を参照している場合の確認
                //

                System.Diagnostics.Debug.Write($" Check: {containerInstance}.{inVariableProperty.Name} => {containerInstance} ... ");

                if (inVariableInstance == varRootContainer)
                {
                    System.Diagnostics.Debug.WriteLine("NG");
                    throw new Exception($"'{varRootContainer}'の{inVariableProperty.Name}が'{varRootContainer}'を参照しています。");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("OK");
                }

                //
                // 入力変数の計算過程を確認
                //

                var inVariablePropertiesOfInVariableInstance = GetVariableList(inVariableInstance);
                foreach (PropertyInfo inVariablePropertyOfInVariableInstance in inVariablePropertiesOfInVariableInstance)
                {
                    foreach (var inVariableInstanceOfInVariableInstance in ExpandValues(inVariablePropertyOfInVariableInstance, inVariableInstance))
                    {
                        System.Diagnostics.Debug.Write($" Check: {containerInstance}.{inVariableProperty.Name} => {inVariableInstance}.{inVariablePropertyOfInVariableInstance.Name} ... ");

                        var refProp = varList.FirstOrDefault(x => x.GetValue(varRootContainer) == inVariableInstanceOfInVariableInstance);
                        if (refProp != null
                        && (IsLinkVariable(refProp.PropertyType) || IsLinkVariable(GetEnumeratedType(refProp.PropertyType))))
                        {
                            System.Diagnostics.Debug.WriteLine("NG");
                            throw new Exception($"{containerInstance}の{inVariablePropertyOfInVariableInstance.Name}が{varRootContainer}の{refProp.Name}を参照しています。");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("OK");
                        }

                        InnerCheckVariableLoop(inVariableInstance, inVariablePropertyOfInVariableInstance, varRootContainer, varList);
                    }
                }
            }
        }

        public static IEnumerable<object> ExpandValues(PropertyInfo propInfo, object target)
        {
            var propValue = propInfo.GetValue(target);
            if( propValue is System.Collections.IEnumerable)
            {
                //配列等なので、個別の値を返す
                var ar = propValue as System.Collections.IEnumerable;
                foreach (var item in ar)
                {
                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }
            else
            {
                //配列等ではない
                if (propValue != null)
                {
                    yield return propValue;
                }
            }
        }

        public static List<System.Reflection.PropertyInfo> GetVariableList(object varContainer)
        {
            var list = new List<System.Reflection.PropertyInfo>();

            var T = varContainer.GetType();
            var properties = T.GetProperties();

            ////LinkVariableの場合はLink先を展開する
            //if (IsLinkVariable(GetEnumeratedType(T)))
            //{
            //    foreach(var element in varContainer as Array)
            //    {
            //        if (element != null)
            //        {
            //            list.AddRange(GetVariableList(element));
            //        }
            //    }
            //    return list;
            //}
            //else if (IsLinkVariable(T) )
            //{
            //    var link = T.GetProperty("Link").GetValue(varContainer);
            //    if (link != null)
            //    {
            //        return GetVariableList(link);
            //    }
            //}

            foreach (var prop in properties)
            {
                if (IsIVariable(prop.PropertyType) || IsIVariable(GetEnumeratedType(prop.PropertyType)))
                {
                    list.Add(prop);
                }
            }

            return list;
        }

        private static bool IsIVariable(Type type)
        {
            if (type == null) return false;

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
            if (type == null) return false;

            foreach (var propT in GetParentTypes(type))
            {
                if (propT.IsGenericType && propT.GetGenericTypeDefinition() == typeof(LinkVariable<>))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If the given <paramref name="type"/> is an array or some other collection
        /// comprised of 0 or more instances of a "subtype", get that type
        /// </summary>
        /// <param name="type">the source type</param>
        /// <returns></returns>
        private static Type GetEnumeratedType(Type type)
        {
            // provided by Array
            var elType = type.GetElementType();
            if (null != elType) return elType;

            // otherwise provided by collection
            var elTypes = type.GetGenericArguments();
            if (elTypes.Length > 0) return elTypes[0];

            // otherwise is not an 'enumerated' type
            return null;
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
