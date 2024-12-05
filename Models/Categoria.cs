using System.Collections.Generic;
using WebCatalogo.Models;

namespace WebCatalogo.Models
{
    public class Categoria
    {

        public int Id_categoria { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }


        //aqui fala que uma categoria pode ter mais de um produto. 
        public ICollection<Produto>? Produtos { get; set; }
    }
}
