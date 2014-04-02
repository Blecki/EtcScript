using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public class ScriptObject
    {
        public Dictionary<String, Object> properties = new Dictionary<string, object>();

        public ScriptObject() { }

        public ScriptObject(ScriptObject cloneFrom)
        {
            foreach (var property in cloneFrom.properties)
                properties.Add(property.Key, property.Value);
        }

		public ScriptObject(Dictionary<String, Object> CloneFrom)
		{
			foreach (var property in CloneFrom)
				properties.Add(property.Key, property.Value);
		}

        public ScriptObject(params Object[] args)
        {
            if (args.Length % 2 != 0) throw new InvalidProgramException("Generic Script Object must be initialized with pairs");
            for (int i = 0; i < args.Length; i += 2)
                SetProperty(args[i].ToString(), args[i + 1]);
        }

        public Object this[String name]
        {
            get
            {
                Object r;
                if (QueryProperty(name, out r)) return r;
                return null;
            }

            set
            {
                SetProperty(name, value);
            }
        }

        public object GetOwnProperty(string name)
        {
            if (properties.ContainsKey(name)) return properties[name];
            else return null;
        }

        public void SetProperty(string Name, Object Value)
        {
            properties.Upsert(Name, Value);
        }

        public void DeleteProperty(String Name)
        {
            if (properties.ContainsKey(Name)) properties.Remove(Name);
        }
        
        public void ClearProperties()
        {
            properties.Clear();
        }

        public bool HasOwnProperty(String name)
        {
            return properties.ContainsKey(name);
        }

        public bool QueryProperty(String name, out Object value)
        {
            value = null;

            if (properties.ContainsKey(name))
            {
                value = properties[name];
                return true;
            }

            if (properties.ContainsKey("@parent"))
            {
                var parent = properties["@parent"] as ScriptObject;
                if (parent != null) return parent.QueryProperty(name, out value);
            }

            return false;
        }

        public ScriptObject CaptureScope()
        {
            var r = new ScriptObject(this);
            r.SetProperty("@parent", this);
            return r;
        }
    }
}
