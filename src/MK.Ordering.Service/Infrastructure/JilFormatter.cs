using System;
using System.IO;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Jil;
using PlainElastic.Net.Serialization;

namespace MK.Ordering.Service.Infrastructure
{
    public class JilSerializer : IJsonSerializer
    {
        readonly Options _jilOptions;

        public JilSerializer()
        {
            _jilOptions = new Options(dateFormat: DateTimeFormat.ISO8601);
        }

        public object Deserialize(string value, Type type)
        {
            var method = typeof(JSON).GetMethod("Deserialize", new Type[] { typeof(string), typeof(Options) });
            var generic = method.MakeGenericMethod(type);
            return generic.Invoke(this, new object[] { value, _jilOptions });
        }

        public string Serialize(object o)
        {
            return JSON.Serialize(o, _jilOptions);
        }
    }

    public class JilFormatter : MediaTypeFormatter
    {
        readonly Options _jilOptions;

        public JilFormatter()
        {
            _jilOptions = new Options(dateFormat: DateTimeFormat.ISO8601);
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
        }

        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            var task= Task<object>.Factory.StartNew(() => (this.DeserializeFromStream(type, readStream)));
            return task;
        }

        private object DeserializeFromStream(Type type, Stream readStream)
        {
            try
            {
                using (var reader = new StreamReader(readStream))
                {
                    var method = typeof(JSON).GetMethod("Deserialize", new Type[] { typeof(TextReader), typeof(Options) });
                    var generic = method.MakeGenericMethod(type);
                    return generic.Invoke(this, new object[] { reader, _jilOptions });
                }
            }
            catch
            {
                return null;
            }
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                JSON.Serialize(value, streamWriter, _jilOptions);
                var task = Task.Factory.StartNew(() => writeStream);
                return task;
            }
        }
    }
}