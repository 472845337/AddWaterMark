
namespace AddWaterMark.DataBase.Services {
    public class ImgFilePathService : TableService<Beans.ImgFilePath> {
        /// <summary>
        /// 显式的静态构造函数用来告诉C#编译器在其内容实例化之前不要标记其类型
        /// </summary>
        static ImgFilePathService() { }

        private ImgFilePathService() { }

        public static ImgFilePathService Instance { get; } = new ImgFilePathService();

    }
}
