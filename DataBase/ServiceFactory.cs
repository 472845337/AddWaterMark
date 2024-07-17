

using AddWaterMark.DataBase.Beans;

namespace AddWaterMark.DataBase.Services {
    public class ServiceFactory {
        private static TableService<ImgFilePath> imgFilePathService;

        public static TableService<ImgFilePath> GetImgFilePathService() {
            if (null == imgFilePathService) {
                imgFilePathService = ImgFilePathService.Instance;
            }
            return imgFilePathService;
        }


        public static S GetService<T, S>() where T : TableData where S : TableService<T> {
            if (typeof(T) == typeof(ImgFilePath)) {
                return GetImgFilePathService() as S;
            } else {
                return null;
            }
        }
    }
}
