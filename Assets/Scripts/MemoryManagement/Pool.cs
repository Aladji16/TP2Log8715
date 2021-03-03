using System;
using System.Collections.Generic;

//    Classe d'allocateur de type Pool

public class Pool
{
    public IComponent[] poolArray;
    public Pool(int size)
    {
        //pool de taille fixe et qui ne rempli pas les trous non plus car nous voulons garder les index correspondants aux memes entities pour tous components
        //cela revient a simplement utiliser un array
        poolArray = new IComponent[size];
    }

    public void setComponent(EntityComponent entityID, IComponent component)
    {
        //mis a jour du poolArray
        poolArray[entityID.id] = component;
    }

    public void remove(EntityComponent entityID)
    {
        //mettre a null
        poolArray[entityID.id] = null;

    }
    public IComponent getComponent(EntityComponent entityID)
    {
        return poolArray[entityID.id];
    }
    public bool hasEntity(EntityComponent entityID)
    {
       return poolArray[entityID.id]!=null;
    }

}