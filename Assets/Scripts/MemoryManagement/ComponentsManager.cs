//#define BAD_PERF // TODO CHANGEZ MOI. Mettre en commentaire pour utiliser votre propre structure

using System;
using UnityEngine;
using System.Collections.Generic;

#if BAD_PERF
using InnerType = System.Collections.Generic.Dictionary<uint, IComponent>;
using AllComponents = System.Collections.Generic.Dictionary<uint, System.Collections.Generic.Dictionary<uint, IComponent>>;
#else
using InnerType = Pool;//approche pool
using AllComponents = System.Collections.Generic.Dictionary<uint, Pool>;
#endif

// Appeler GetHashCode sur un Type est couteux. Cette classe sert a precalculer le hashcode
public static class TypeRegistry<T> where T : IComponent
{
    public static uint typeID = (uint)Mathf.Abs(default(T).GetRandomNumber()) % ComponentsManager.maxEntities;
}

public class Singleton<V> where V : new()
{
    private static bool isInitiated = false;
    private static V _instance;
    public static V Instance
    {
        get
        {
            if (!isInitiated)
            {
                isInitiated = true;
                _instance = new V();
            }
            return _instance;
        }
    }
    protected Singleton() { }
}

internal class ComponentsManager : Singleton<ComponentsManager>
{
    private AllComponents _allComponents = new AllComponents();

    public const int maxEntities = 2000;

    public void DebugPrint()
    {
        string toPrint = "";
        var allComponents = Instance.DebugGetAllComponents();
        foreach (var type in allComponents)
        {
            toPrint += $"{type}: \n";
#if !BAD_PERF
            //foreach (var component in type)
#else
            foreach (var component in type.Value)
#endif
            {
#if BAD_PERF
                toPrint += $"\t{component.Key}: {component.Value}\n";
#else
                //toPrint += $"\t{component}: {component}\n";
#endif
            }
            //toPrint += "\n";
        }
        Debug.Log(toPrint);
    }

    // CRUD
    public void SetComponent<T>(EntityComponent entityID, IComponent component) where T : IComponent
    {
        //if (!_allComponents.ContainsKey(TypeRegistry<T>.typeID))
        //{
        //    //_allComponents[TypeRegistry<T>.typeID] = new Dictionary<uint, IComponent>();
        //    _allComponents[TypeRegistry<T>.typeID] = new InnerType();
        //}
        //_allComponents[TypeRegistry<T>.typeID][entityID] = component;  

        if (!_allComponents.ContainsKey(TypeRegistry<T>.typeID))
        {
            //si le pool de ce component n'existe pas encore, creer un nouveau
            _allComponents[TypeRegistry<T>.typeID] = new InnerType(ECSManager.Instance.Config.numberOfShapesToSpawn);
        }
       
        _allComponents[TypeRegistry<T>.typeID].setComponent(entityID, component);
       

    }
    public void RemoveComponent<T>(EntityComponent entityID) where T : IComponent
    {
        //_allComponents[TypeRegistry<T>.typeID].Remove(entityID);

        _allComponents[TypeRegistry<T>.typeID].remove(entityID);
    }
    public T GetComponent<T>(EntityComponent entityID) where T : IComponent
    {
        //return (T)_allComponents[TypeRegistry<T>.typeID][entityID];
        return (T)_allComponents[TypeRegistry<T>.typeID].getComponent(entityID);
    }
    public bool TryGetComponent<T>(EntityComponent entityID, out T component) where T : IComponent
    {
        if (_allComponents.ContainsKey(TypeRegistry<T>.typeID))
        {
            //if (_allComponents[TypeRegistry<T>.typeID].ContainsKey(entityID))
            //{
            //    component = (T)_allComponents[TypeRegistry<T>.typeID][entityID];
            //    return true;
            //}
            if (_allComponents[TypeRegistry<T>.typeID].hasEntity(entityID))
            {
                component = (T)_allComponents[TypeRegistry<T>.typeID].getComponent(entityID);
                return true;
            }
        }
        component = default;
        return false;
    }

    public bool EntityContains<T>(EntityComponent entity) where T : IComponent
    {
        //return _allComponents[TypeRegistry<T>.typeID].ContainsKey(entity);
        return _allComponents[TypeRegistry<T>.typeID].hasEntity(entity);
    }

    public void ClearComponents<T>() where T : IComponent
    {
        if (!_allComponents.ContainsKey(TypeRegistry<T>.typeID))
        {
            _allComponents.Add(TypeRegistry<T>.typeID, new InnerType(ECSManager.Instance.Config.numberOfShapesToSpawn));
        }
        else
        {
            _allComponents[TypeRegistry<T>.typeID] = new InnerType(ECSManager.Instance.Config.numberOfShapesToSpawn);
        }
    }

    public void ForEach<T1>(Action<EntityComponent, T1> lambda) where T1 : IComponent
    {

        /*var allEntities = _allComponents[TypeRegistry<EntityComponent>.typeID].Values;
        foreach (EntityComponent entity in allEntities)
        {
            if (!_allComponents[TypeRegistry<T1>.typeID].ContainsKey(entity))
            {
                continue;
            }
            lambda(entity, (T1)_allComponents[TypeRegistry<T1>.typeID][entity]);
        }*/
        IComponent[] allEntities = _allComponents[TypeRegistry<EntityComponent>.typeID].poolArray;
        IComponent[] t1_poolArray = _allComponents[TypeRegistry<T1>.typeID].poolArray;
        for (uint i = 0; i < ECSManager.Instance.Config.numberOfShapesToSpawn; i++)
        {
            if (t1_poolArray[i] == null)
            {
                continue;
            }
            lambda((EntityComponent)allEntities[i], (T1)t1_poolArray[i]);
        }
    }
    public void ForEach<T1, T2>(Action<EntityComponent, T1, T2> lambda) where T1 : IComponent where T2 : IComponent
    {
        /* var allEntities = _allComponents[TypeRegistry<EntityComponent>.typeID].Values;
        foreach(EntityComponent entity in allEntities)
        {
            if (!_allComponents[TypeRegistry<T1>.typeID].ContainsKey(entity) ||
                !_allComponents[TypeRegistry<T2>.typeID].ContainsKey(entity)
                )
            {
                continue;
            }
            lambda(entity, (T1)_allComponents[TypeRegistry<T1>.typeID][entity], (T2)_allComponents[TypeRegistry<T2>.typeID][entity]);
        } */
        IComponent[] allEntities = _allComponents[TypeRegistry<EntityComponent>.typeID].poolArray;
        IComponent[] t1_poolArray = _allComponents[TypeRegistry<T1>.typeID].poolArray;
        IComponent[] t2_poolArray = _allComponents[TypeRegistry<T2>.typeID].poolArray;
        
        for (uint i = 0; i < ECSManager.Instance.Config.numberOfShapesToSpawn; i++)
        {
            if (t1_poolArray[i] == null || t2_poolArray[i] == null)
            {
                continue;
            }
            lambda((EntityComponent)allEntities[i], (T1)t1_poolArray[i], (T2)t2_poolArray[i]);
        }
    }

    public void ForEach<T1, T2, T3>(Action<EntityComponent, T1, T2, T3> lambda) where T1 : IComponent where T2 : IComponent where T3 : IComponent
    {

        /* var allEntities = _allComponents[TypeRegistry<EntityComponent>.typeID].Values;
        foreach (EntityComponent entity in allEntities)
        {
            if (!_allComponents[TypeRegistry<T1>.typeID].ContainsKey(entity) ||
                !_allComponents[TypeRegistry<T2>.typeID].ContainsKey(entity) ||
                !_allComponents[TypeRegistry<T3>.typeID].ContainsKey(entity)
                )
            {
                continue;
            }
            lambda(entity, (T1)_allComponents[TypeRegistry<T1>.typeID][entity], (T2)_allComponents[TypeRegistry<T2>.typeID][entity], (T3)_allComponents[TypeRegistry<T3>.typeID][entity]);
        } */
        IComponent[] allEntities = _allComponents[TypeRegistry<EntityComponent>.typeID].poolArray;
        IComponent[] t1_poolArray = _allComponents[TypeRegistry<T1>.typeID].poolArray;
        IComponent[] t2_poolArray = _allComponents[TypeRegistry<T2>.typeID].poolArray;
        IComponent[] t3_poolArray = _allComponents[TypeRegistry<T3>.typeID].poolArray;
        for (uint i = 0; i < ECSManager.Instance.Config.numberOfShapesToSpawn; i++)
        {
            if (t1_poolArray[i] == null || t2_poolArray[i] == null || t3_poolArray[i] == null)
            {
                continue;
            }
            lambda((EntityComponent)allEntities[i], (T1)t1_poolArray[i], (T2)t2_poolArray[i], (T3)t3_poolArray[i]);
        }
    }

    public void ForEach<T1, T2, T3, T4>(Action<EntityComponent, T1, T2, T3, T4> lambda) where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
    {

        /* var allEntities = _allComponents[TypeRegistry<EntityComponent>.typeID].Values;
       foreach (EntityComponent entity in allEntities)
       {
           if (!_allComponents[TypeRegistry<T1>.typeID].ContainsKey(entity) ||
               !_allComponents[TypeRegistry<T2>.typeID].ContainsKey(entity) ||
               !_allComponents[TypeRegistry<T3>.typeID].ContainsKey(entity) ||
               !_allComponents[TypeRegistry<T4>.typeID].ContainsKey(entity)
               )
           {
               continue;
           }
           lambda(entity, (T1)_allComponents[TypeRegistry<T1>.typeID][entity], (T2)_allComponents[TypeRegistry<T2>.typeID][entity], (T3)_allComponents[TypeRegistry<T3>.typeID][entity], (T4)_allComponents[TypeRegistry<T4>.typeID][entity]);
       } */
        IComponent[] allEntities = _allComponents[TypeRegistry<EntityComponent>.typeID].poolArray;
        IComponent[] t1_poolArray = _allComponents[TypeRegistry<T1>.typeID].poolArray;
        IComponent[] t2_poolArray = _allComponents[TypeRegistry<T2>.typeID].poolArray;
        IComponent[] t3_poolArray = _allComponents[TypeRegistry<T3>.typeID].poolArray;
        IComponent[] t4_poolArray = _allComponents[TypeRegistry<T4>.typeID].poolArray;


        for (uint i = 0; i < ECSManager.Instance.Config.numberOfShapesToSpawn; i++)
        {
            if (t1_poolArray[i] == null || t2_poolArray[i] == null || t3_poolArray[i] == null || t4_poolArray[i] == null)
            {
                continue;
            }
            lambda((EntityComponent)allEntities[i], (T1)t1_poolArray[i], (T2)t2_poolArray[i], (T3)t3_poolArray[i], (T4)t4_poolArray[i]);
        }
    }

    public AllComponents DebugGetAllComponents()
    {
        return _allComponents;
    }
}
