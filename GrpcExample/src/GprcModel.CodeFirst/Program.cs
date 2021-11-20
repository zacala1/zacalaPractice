using GprcModel1.Models;
using GprcModel1.Services;
using ProtoBuf;
using ProtoBuf.Grpc.Reflection;
using ProtoBuf.Meta;
using System;
using System.IO;

namespace GprcModel1
{
    class Program
    {
        static void Main(string[] args)
        {
            ProtoFileWrite<HealthData>();
            SchemaFileWrite<IWeatherService>();
        }

        static void ProtoFileWrite<T>() where T : class
        {
            string subPath = "./proto";
            System.IO.DirectoryInfo dInfo = new DirectoryInfo(subPath);
            if (!dInfo.Exists) System.IO.Directory.CreateDirectory(subPath);
            string protoFileName = string.Format("{0}.proto", typeof(T).Name);
            string protoFilePath = Path.Combine(subPath, protoFileName);
            string objectProtoFileData = Serializer.GetProto<T>(ProtoSyntax.Proto3);
            using (StreamWriter sw = new StreamWriter(protoFilePath))
            {
                sw.WriteLine(objectProtoFileData);
            }
        }

        static void SchemaFileWrite<T>() where T : class
        {
            string subPath = "./proto";
            System.IO.DirectoryInfo dInfo = new DirectoryInfo(subPath);
            if (!dInfo.Exists) System.IO.Directory.CreateDirectory(subPath);

            string name = typeof(T).Name;
            if (name.StartsWith("I")) name = name.Substring(1);
            if (name.EndsWith("Service")) name = name.Substring(0, name.Length - 7);
            if (name.EndsWith("Async")) name = name.Substring(0, name.Length - 5);

            string protoFileName = string.Format("{0}.proto", name);
            string protoFilePath = Path.Combine(subPath, protoFileName);
            var generator = new SchemaGenerator
            {
                ProtoSyntax = ProtoSyntax.Proto3
            };
            var objectProtoFileData = generator.GetSchema<T>(); // there is also a non-generic overload that takes Type
            using (StreamWriter sw = new StreamWriter(protoFilePath))
            {
                sw.WriteLine(objectProtoFileData);
            }
        }
    }
}
