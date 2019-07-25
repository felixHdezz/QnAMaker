using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QnABot.DataServices
{
    public class ISingletonInstance<T> where T : class, new()
    {
        // Crea una nueva instacia del Objecto T
        private static T EntityIntance = null;

        // Varificar si la instacia es llamado desde multiples hilos
        private static object root = new object();

        // Propiedad de tipo Entity
        public static T GetEntityIntance
        {
            get
            {
                // Si es null, instancia el nuevo objeto
                if (EntityIntance == null)
                {
                    // Verifica si esta bloqueado las instancia
                    lock (root)
                    {
                        if (EntityIntance == null)
                        {
                            // Hace una nueva instacia al objeto
                            EntityIntance = new T();
                        }
                    }
                }

                // Develve el objeto
                return EntityIntance;
            }
        }
    }
}