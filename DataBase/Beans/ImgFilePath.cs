namespace AddWaterMark.DataBase.Beans {
    [Table("t_img_file_path")]
    public class ImgFilePath : TableData {
        // 路径
        [TableParam("file_path", "VARCHAR")]
        public string FilePath { get; set; }
        // 水印文本
        [TableParam("water_mark", "VARCHAR")]
        public string WaterMark { get; set; }
    }
}
