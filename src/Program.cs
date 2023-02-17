using MongoDB.Bson;
using MongoDB.Driver;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Linq;

internal static class Program
{
  private static readonly string[] names = new string[] { "Osawa", "Usukura", "Suzuki" };
  private static readonly string[] professions = new string[] { "Programmer", "Engineer", "Designer" };
  private static readonly int[] ages = new int[] { 15, 20, 25, 30, 35 };

  private static IMongoDatabase? database;

  internal static int Main()
  {
    try
    {
      string xml_path = "./config.xml";
      string xsd_path = "./config.xsd";

      if (File.Exists(xml_path) == false)
      {
        Console.WriteLine("Could not find XML configuration file.");
        return 1;
      }
      if (File.Exists(xsd_path) == false)
      {
        Console.WriteLine("Could not find XSD configuration file.");
        return 1;
      }

      string xml_content = File.ReadAllText(xml_path);
      string xsd_content = File.ReadAllText(xsd_path);

      bool validation_check = true;

      //XMLスキーマオブジェクトの生成
      XmlSchema schema = new();
      using (StringReader stringReader = new(xsd_content))
      {
        schema = XmlSchema.Read(stringReader, null)!;
      }
      // スキーマの追加
      XmlSchemaSet schemaSet = new();
      schemaSet.Add(schema);

      // XML文書の検証を有効化
      XmlReaderSettings settings = new()
      {
        ValidationType = ValidationType.Schema,
        Schemas = schemaSet
      };
      settings.ValidationEventHandler += (object? sender, ValidationEventArgs e) => {
        if (e.Severity == XmlSeverityType.Warning)
        {
          Console.WriteLine($"Validation Warning ({e.Message})");
        }
        if (e.Severity == XmlSeverityType.Error)
        {
          Console.WriteLine($"Validation Error ({e.Message})");
          validation_check = false;
        }
      };

      // XMLデータの読み込み
      using (StringReader stringReader = new(xml_content))
      using (XmlReader xmlReader = XmlReader.Create(stringReader, settings))
      {
        while (xmlReader.Read()) { }
      }

      if (validation_check == false)
      {
        Console.WriteLine("Validation failed...");
        return 1;
      }

      // 設定ファイルからデータを取得
      XDocument xml_document = XDocument.Parse(xml_content);
      XElement config = xml_document.Element("config")!;
      string database_host = config.Element("database_host")?.Value!;
      int database_port = int.Parse(config.Element("database_port")?.Value!);
      string database_username = config.Element("database_username")?.Value!;
      string database_password = config.Element("database_password")?.Value!;
      string database_database = config.Element("database_database")?.Value!;
      string database_collection = config.Element("database_collection")?.Value!;

      // コンソール画面に表示
      Console.WriteLine($"database_host: {database_host}");
      Console.WriteLine($"database_port: {database_port}");
      Console.WriteLine($"database_username: {database_username}");
      Console.WriteLine($"database_password: {database_password}");
      Console.WriteLine($"database_database: {database_database}");
      Console.WriteLine($"database_collection: {database_collection}");

      // MongoDB接続文字列を指定してクライアントを作成
      var credential = MongoCredential.CreateCredential(database_database, database_username, database_password);
      var mongo_settings = new MongoClientSettings
      {
        Credential = credential,
        Server = new MongoServerAddress(database_host, database_port)
      };
      var client = new MongoClient(mongo_settings);
      database = client.GetDatabase(database_database);

      Console.Write("Press any key to fetch data..."); Console.ReadKey(); Console.WriteLine();
      FetchData(database_collection);

      Console.Write("Press any key to insert data..."); Console.ReadKey(); Console.WriteLine();
      InsertData(database_collection);

      Console.Write("Press any key to find data..."); Console.ReadKey(); Console.WriteLine();
      FindData(database_collection);

      Console.Write("Press any key to update data..."); Console.ReadKey(); Console.WriteLine();
      UpdateData(database_collection);

      Console.Write("Press any key to delete data..."); Console.ReadKey(); Console.WriteLine();
      DeleteData(database_collection);

      Console.Write("Press any key to fetch data..."); Console.ReadKey(); Console.WriteLine();
      FetchData(database_collection);

      return 0;
    } catch (Exception ex)
    {
      Console.WriteLine(ex.ToString());
      return 1;
    }
  }

  internal static void InsertData(string collection_name)
  {
    // 名前をランダムに選択
    var name = names[new Random().Next(names.Length)];

    // 職業をランダムに選択
    var profession = professions[new Random().Next(professions.Length)];

    // 年齢をランダムに選択
    var age = ages[new Random().Next(ages.Length)];

    Console.WriteLine($"★★★ Inserting data... ★★★");
    Console.WriteLine($"name: {name}");
    Console.WriteLine($"profession: {profession}");
    Console.WriteLine($"age: {age}");
    Console.WriteLine($"★★★ ★★★ ★★★ ★★★ ★★★");

    // ドキュメントを作成
    var document = new BsonDocument
      {
        {"name", name},
        {"profession", profession},
        {"age", age},
      };

    // コレクションを取得
    var collection = database!.GetCollection<BsonDocument>(collection_name);

    // ドキュメントを挿入
    collection.InsertOne(document);
  }

  internal static void FindData(string collection_name)
  {
    // コレクションを取得
    var collection = database!.GetCollection<BsonDocument>(collection_name);

    // 抽出する名前をランダムに選択
    var name = names[new Random().Next(names.Length)];

    // 条件を指定してドキュメントを検索
    var filter = Builders<BsonDocument>.Filter.Eq("name", name);
    var result = collection.Find(filter).ToList();

    // 検索結果を表示
    Console.WriteLine($"----- Finding data... ({name}) -----");
    foreach (var document in result)
    {
      Console.WriteLine(document.ToJson());
    }
    Console.WriteLine($"----- ----- ----- ----- ----- -----");
  }

  internal static void UpdateData(string collection_name)
  {
    // 変更前の年齢をランダムに選択
    var old_age = ages[new Random().Next(ages.Length)];

    // 変更後の年齢をランダムに選択
    var new_age = ages[new Random().Next(ages.Length)];

    // データの更新
    var collection = database!.GetCollection<BsonDocument>(collection_name);
    var filter = Builders<BsonDocument>.Filter.Eq("age", old_age);
    var update = Builders<BsonDocument>.Update.Set("age", new_age);
    collection.UpdateMany(filter, update);

    // 変更前のデータを表示
    Console.WriteLine($"----- Updating data... ({old_age} -> {new_age}) -----");
    Console.WriteLine($"----- ----- ----- ----- ----- -----");
  }

  internal static void DeleteData(string collection_name)
  {
    // 削除する年齢をランダムに選択
    var age = ages[new Random().Next(ages.Length)];

    // データの削除
    var collection = database!.GetCollection<BsonDocument>(collection_name);
    var filter = Builders<BsonDocument>.Filter.Eq("age", age);
    collection.DeleteMany(filter);

    // 削除したデータを表示
    Console.WriteLine($"----- Deleting data... ({age}) -----");
    Console.WriteLine($"----- ----- ----- ----- ----- -----");
  }

  internal static void FetchData(string collection_name)
  {
    // 全てのデータを取得
    var collection = database!.GetCollection<BsonDocument>(collection_name);
    var result = collection.Find(new BsonDocument()).ToList();

    // 検索結果を表示
    Console.WriteLine($"===== Fetching data... =====");
    foreach (var document in result)
    {
      Console.WriteLine(document.ToJson());
    }
    Console.WriteLine($"===== ===== ===== ===== =====");
  }
}
