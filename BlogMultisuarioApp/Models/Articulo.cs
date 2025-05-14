using Amazon.DynamoDBv2.DataModel;

namespace BlogMultisuarioApp.Models
{
    [DynamoDBTable("Articulos")]
    public class Articulo
    {
        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBRangeKey]
        public string ArticuloId { get; set; }

        public string Titulo { get; set; }

        public string Contenido { get; set; }

        public string Autor { get; set; }

        public Estado Estado { get; set; }

        public string FechaCreacion { get; set; }

        public string FechaActualizacion { get; set; }

        public string ImagenNombre { get; set; }
    }

    public enum Estado
    {
        Borrador, Publicado
    }
}
