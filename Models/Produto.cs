namespace WebCatalogo.Models
{
    public class Produto
    {

        public int Id_produto { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Precco   { get; set; }
        public DateTime Data_Compra {  get; set; }
        public int Estoque { get; set; }


        //aqui estu relacionando produto com categoria
        public int Id_categoria { get; set; }
        public Categoria? Categoria { get; set; }
    }
}
