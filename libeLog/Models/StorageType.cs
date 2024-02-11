namespace libeLog.Models
{
    public class StorageType
    {
        public enum Types
        {
            Database, Excel, All
        }

        public Types Type { get; set; }
        public string Name => Type switch
        {
            Types.Database => "База данных",
            Types.Excel => "Книга Excel с макросами",
            Types.All => "База данных + Книга Excel с макросами",
            _ => "",
        }; 

        public StorageType(Types type)
        {
            Type = type;
        }
    }
}
