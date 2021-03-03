using System;
using System.Collections.Generic;
using System.Linq;

//    Classe d'allocateur de type Sequential Pool

public class SequentialPool
{
    private IComponent[] poolArray;
    private System.Collections.Generic.Dictionary<EntityComponent, int> indirectionTable;
    private System.Collections.Generic.Dictionary<int, EntityComponent> inverseTable;
    private int lastIndex;
    public SequentialPool(int size)
    {
        //Tableau de sequential pool avec les components 
        poolArray = new IComponent[size];
        //Dictionnaire pour la table d'indirection (entityIndex to poolIndex)
        indirectionTable = new System.Collections.Generic.Dictionary<EntityComponent, int>();
        //Dictionnaire pour la table d'indirection dans la direction inverse (poolIndex to entityIndex)
        inverseTable = new System.Collections.Generic.Dictionary<int, EntityComponent>();
        lastIndex = -1;
    }

    public void add(EntityComponent entityID, IComponent component)
    {
        //ajoute le nouveau entity a la fin du poolArray
        lastIndex++;
        indirectionTable[entityID] = lastIndex;
        inverseTable[lastIndex] = entityID;
        poolArray[lastIndex] = component;
    }

    public void update(EntityComponent entityID, IComponent component)
    {
        //mis a jour du poolArray utilisant la table d'indirection
        poolArray[indirectionTable[entityID]] = component;
    }

    public void remove(EntityComponent entityID)
    {
        if (indirectionTable[entityID] == lastIndex)
        {
            //si l'entity a enlever est le dernier, mettre a null les index et valeurs du dernier entity
            poolArray[lastIndex] = null;
            indirectionTable.Remove(entityID);
            inverseTable.Remove(lastIndex);
            lastIndex--;
        }
        else
        {
            //si l'entity a enlever n'est pas le dernier de l'array, on enlever l'entity target et swap le dernier pour remplir sa place. Ceci permet de ne pas avoir de valeurs vides lors de 
            poolArray[indirectionTable[entityID]] = poolArray[lastIndex];//mettre la valeur du dernier entity a la position target
            poolArray[lastIndex] = null;//supprimer la valeur du dernier entity
            indirectionTable[inverseTable[lastIndex]] = indirectionTable[entityID];//mettre le bon poolIndex associé au entityID
            inverseTable[indirectionTable[entityID]] = inverseTable[lastIndex];//faire le meme pour le tableau inversé
            indirectionTable.Remove(entityID);//supprimer les valeurs antecedantes
            inverseTable.Remove(lastIndex);
            lastIndex--;
        }
    }
    public IComponent getComponent(EntityComponent entityID)
    {
        return poolArray[indirectionTable[entityID]];
    }
    public bool hasEntity(EntityComponent entityID)
    {
       return indirectionTable.ContainsKey(entityID);
    }
    public EntityComponent[] getEntities()
    {
        return indirectionTable.Keys.ToArray();
    }
}