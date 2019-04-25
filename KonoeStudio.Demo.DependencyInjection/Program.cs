using KonoeStudio.Libs.DependencyInjection;
using KonoeStudio.Libs.DependencyInjection.Interfaces;

namespace KonoeStudio.Demo.DependencyInjection
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var baseClass = new BaseClass();
            baseClass.Demo1();
            baseClass.Demo2();
            baseClass.Demo3();
        }
    }

    public class BaseClass
    {
        public void Demo1()
        {
            // DiVendorの初期化
            DiVendor diVendor = new DiVendor();

            // InoMeanInterfaceが要求されたら、NoMeanClassを渡すように登録する
            diVendor.Register<INoMeanInterface, NoMeanClass>();

            // HaveNoMeanConstructorが要求されたら、HaveNoMeanConstructorを渡すように登録する（登録順序は自由）
            diVendor.Register<HaveNoMeanConstructor>();

            // HaveNoMeanConstructorを調達してもらう。コンストラクタで要求するINoMeanInterfaceは自動で解決する
            HaveNoMeanConstructor haveNoMeanConstructor = diVendor.Procure<HaveNoMeanConstructor>();
        }

        public void Demo2()
        {
            // DiVendorの初期化
            DiVendor diVendor = new DiVendor();

            // DiArchitectはIDiBlueprintを作成できる
            IDiArchitect diArchitect = new DiArchitect();

            // LiteralConstructorクラスのコンストラクタに必要な値をDiArgumentInfoクラスで作成する
            IDiArgumentInfo arg1 = new DiArgumentInfo(typeof(int), 10);

            // 作成方法は二通りある。必ずコンストラクタ引数の型と値をセットにすること
            IDiArgumentInfo arg2 = new DiArgumentInfo<string>("TEST");

            // LiteralConstructorクラスはシングルトンではなく、必要とされる度に毎回newする
            bool isSingleton = false;
            // LiteralConstructorクラスはRegister時にインスタンスを作成せず、必要とされる時にnewする
            bool isLazyinitialized = true;

            // DiArchitectにLiteralConstructorを供給するのに必要な設計図(Blueprint)を作成してもらう
            IDiBlueprint blueprint =
                diArchitect.CreateBlueprint<LiteralConstructor>(isSingleton, isLazyinitialized, arg1, arg2);

            // DiVendorにILiteralConstructorが要求されたら、LiteralConstructorを渡すように設計図付きで登録する
            diVendor.Register<ILiteralConstructor, LiteralConstructor>(blueprint);

            // IDependedConstructorを毎回生成するように登録する
            diVendor.Register<IDependedConstructor, DependedConstructor>(false);

            // Demo1と同様にInoMeanInterfaceを登録する（登録順序は自由）
            diVendor.Register<INoMeanInterface, NoMeanClass>();

            // IDependedConstructorを調達してもらう。下記2つのインスタンス中のINoMeanInterfaceは同一のインスタンス
            // だが、IDependedConstructorとILiteralConstructorはそれぞれ異なるインスタンスになっている(isSingleton = false としたから)
            IDependedConstructor depended1 = diVendor.Procure<IDependedConstructor>();
            IDependedConstructor depended2 = diVendor.Procure<IDependedConstructor>();

            // もしIDependedConstructorをシングルトンとして登録した場合は、その中に設定されるILiteralConstructorも単一になってしまうため注意
        }

        public void Demo3()
        {
            // Demo2と同様の設定をする
            DiVendor diVendor = new DiVendor();
            IDiArchitect diArchitect = new DiArchitect();
            IDiArgumentInfo arg1 = new DiArgumentInfo(typeof(int), 10);
            IDiArgumentInfo arg2 = new DiArgumentInfo<string>("TEST");
            bool isSingleton = false;
            bool isLazyinitialized = true;
            IDiBlueprint blueprint =
                diArchitect.CreateBlueprint<LiteralConstructor>(isSingleton, isLazyinitialized, arg1, arg2);
            diVendor.Register<ILiteralConstructor, LiteralConstructor>(blueprint);
            diVendor.Register<IDependedConstructor, DependedConstructor>(false);
            diVendor.Register<INoMeanInterface, NoMeanClass>();

            // IComplexConstructorの登録を行う。ComplexConstructorの複雑なコンストラクタ引数を登録する必要がある
            // 値にnullを入れたい場合は、nullを指定した上で第二引数をtrueにする
            IDiArgumentInfo complexArg1 = new DiArgumentInfo<INoMeanInterface>(null);

            // 第二引数をfalseにした状態で値にnullを入れると、DIコンテナに自動解決させる設定とみなす
            IDiArgumentInfo complexArg2 = new DiArgumentInfo<ILiteralConstructor>(null);
            
            // 何もいれなければ自動でインスタンス解決する設定になる
            IDiArgumentInfo complexArg3 = new DiArgumentInfo<IDependedConstructor>();
            IDiArgumentInfo complexArg4 = new DiArgumentInfo<int>(1);

            // ComplexConstructorの設計図を作ってもらう。指定するクラスは実際に作ってもらいたい（コンストラクタを持つ）クラスにすること
            IDiBlueprint blueprintForComplex =
                diArchitect.CreateBlueprint<ComplexConstructor>(true, true, complexArg1, complexArg2, complexArg3,
                    complexArg4);

            // DiVendorに設計図つきで登録する
            diVendor.Register<IComplexConstructor, ComplexConstructor>(blueprintForComplex);

            // IComplexConstructorを調達してもらう。
            IComplexConstructor complexConstructor = diVendor.Procure<IComplexConstructor>();

            // complexConstructor.DependedConstructor は null
        }
    }

    public interface INoMeanInterface
    {
        // 何もないインターフェース
    }

    public class NoMeanClass : INoMeanInterface
    {
        // 何もないインターフェースを継承するクラス
    }

    public class HaveNoMeanConstructor
    {
        // NoMeanInterfaceをコンストラクタでDIしてもらうクラス
        public INoMeanInterface NoMean { get; set; }

        public HaveNoMeanConstructor(INoMeanInterface noMean)
        {
            NoMean = noMean;
        }
    }

    public interface ILiteralConstructor
    {
        // intとstringだけもつインターフェース
        int Num { get; }
        string Str { get; }
    }

    public class LiteralConstructor : ILiteralConstructor
    {
        // intとstringをDIしてもらうクラス
        public LiteralConstructor(int num, string str)
        {
            Num = num;
            Str = str;
        }

        public int Num { get; }
        public string Str { get; }
    }
    public interface IDependedConstructor
    {
        // INoMeanInterfaceとILiteralConstructorをもつインターフェース
        INoMeanInterface NoConstructor { get; }
        ILiteralConstructor LiteralConstructor { get; }
    }
    public class DependedConstructor : IDependedConstructor
    {
        // INoMeanInterfaceとILiteralConstructorをDIしてもらうクラス
        public INoMeanInterface NoConstructor { get; }
        public ILiteralConstructor LiteralConstructor { get; }

        public DependedConstructor(INoMeanInterface noConstructor, ILiteralConstructor literalConstructor)
        {
            NoConstructor = noConstructor;
            LiteralConstructor = literalConstructor;
        }
    }
    public interface IComplexConstructor
    {
        // INoMeanInterfaceとILiteralConstructorと
        // IDependedConstructorとintをもつインターフェース
        INoMeanInterface NoConstructor { get; }
        ILiteralConstructor LiteralConstructor { get; }
        IDependedConstructor DependedConstructor { get; }
        int Arg1 { get; }
    }
    public class ComplexConstructor : IComplexConstructor
    {
        // INoMeanInterfaceとILiteralConstructorと
        // IDependedConstructorとintをDIしてもらうクラス
        public INoMeanInterface NoConstructor { get; }
        public ILiteralConstructor LiteralConstructor { get; }
        public IDependedConstructor DependedConstructor { get; }
        public int Arg1 { get; }

        public ComplexConstructor(INoMeanInterface noConstructor, ILiteralConstructor literalConstructor, IDependedConstructor dependedConstructor, int arg1)
        {
            NoConstructor = noConstructor;
            LiteralConstructor = literalConstructor;
            DependedConstructor = dependedConstructor;
            Arg1 = arg1;
        }
    }
}
