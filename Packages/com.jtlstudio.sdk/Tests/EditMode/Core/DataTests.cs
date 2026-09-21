using System;
using JTLStudio.SDK.Tests.Fakes;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests.Core
{
    public class DataTests
    {
        [Serializable]
        public class Profile
        {
            public string name = "";
            public int level;
            public float progress;
        }

        private TestSettingsBuilder _builder;
        private FakeDataProvider _data;

        [SetUp]
        public void SetUp()
        {
            JTLSDK.Destroy();
            _data = new FakeDataProvider();
            _builder = new TestSettingsBuilder { Data = _data };
        }

        [TearDown]
        public void TearDown()
        {
            _builder.Cleanup();
        }

        [Test]
        public void EmptyLoadIsReadyAndWritable()
        {
            JTLSDK.Create(_builder.Build());

            Assert.AreEqual(DataState.Empty, JTLSDK.Data.LoadState);
            Assert.AreEqual(ModuleState.Ready, JTLSDK.Data.State);

            JTLSDK.Data.SetInt("Money", 5);
            Assert.AreEqual(5, JTLSDK.Data.GetInt("Money"));
            Assert.IsTrue(JTLSDK.Data.IsDirty);
        }

        [Test]
        public void ValuesRoundTripThroughFlushAndLoad()
        {
            JTLSDK.Create(_builder.Build());
            JTLSDK.Data.SetInt("Money", 1200);
            JTLSDK.Data.SetFloat("Volume", 0.5f);
            JTLSDK.Data.SetBool("Tutorial", true);
            JTLSDK.Data.SetString("Name", "Игрок \"один\"");
            JTLSDK.Data.SetObject("Profile", new Profile { name = "Player", level = 27, progress = 0.75f });
            bool? flushed = null;

            JTLSDK.Data.Flush(success => flushed = success);

            Assert.IsTrue(flushed);
            Assert.IsFalse(JTLSDK.Data.IsDirty);
            Assert.AreEqual(1, _data.SaveCount);

            JTLSDK.Destroy();
            FakeDataProvider reloaded = new FakeDataProvider { LoadResult = Providers.DataLoadResult.Loaded, LoadPayload = _data.LastSaved };
            _builder.Data = reloaded;
            JTLSDK.Create(_builder.Build());

            Assert.AreEqual(DataState.Loaded, JTLSDK.Data.LoadState);
            Assert.AreEqual(1200, JTLSDK.Data.GetInt("Money"));
            Assert.AreEqual(0.5f, JTLSDK.Data.GetFloat("Volume"), 0.0001f);
            Assert.IsTrue(JTLSDK.Data.GetBool("Tutorial"));
            Assert.AreEqual("Игрок \"один\"", JTLSDK.Data.GetString("Name"));

            Profile profile = JTLSDK.Data.GetObject<Profile>("Profile");
            Assert.AreEqual("Player", profile.name);
            Assert.AreEqual(27, profile.level);
            Assert.AreEqual(0.75f, profile.progress, 0.0001f);
        }

        [Test]
        public void MissingKeysReturnDefaults()
        {
            JTLSDK.Create(_builder.Build());

            Assert.AreEqual(7, JTLSDK.Data.GetInt("Missing", 7));
            Assert.AreEqual("x", JTLSDK.Data.GetString("Missing", "x"));
            Assert.IsNull(JTLSDK.Data.GetObject<Profile>("Missing"));
            Assert.IsFalse(JTLSDK.Data.HasKey("Missing"));
        }

        [Test]
        public void WriteBeforeReadyIsIgnored()
        {
            _data.CompleteImmediately = false;
            JTLSDK.Create(_builder.Build());

            JTLSDK.Data.SetInt("Money", 5);
            _data.Complete(Providers.ProviderState.Ready);

            Assert.AreEqual(0, JTLSDK.Data.GetInt("Money"));
            Assert.IsFalse(JTLSDK.Data.IsDirty);
        }

        [Test]
        public void FailedLoadBlocksWritingToStorage()
        {
            _data.LoadResult = Providers.DataLoadResult.Failed;
            JTLSDK.Create(_builder.Build());
            bool? flushed = null;

            JTLSDK.Data.SetInt("Money", 5);
            JTLSDK.Data.Flush(success => flushed = success);

            Assert.AreEqual(DataState.Failed, JTLSDK.Data.LoadState);
            Assert.AreEqual(5, JTLSDK.Data.GetInt("Money"));
            Assert.IsFalse(flushed);
            Assert.AreEqual(0, _data.SaveCount);
        }

        [Test]
        public void FailedLoadIsRetriedAndReplacesMemory()
        {
            _data.LoadResult = Providers.DataLoadResult.Failed;
            JTLSDK.Create(_builder.Build());
            JTLSDK.Data.SetInt("Money", 5);

            _data.LoadResult = Providers.DataLoadResult.Loaded;
            _data.LoadPayload = "{\"format\":1,\"revision\":3,\"values\":{\"Money\":99}}";
            JTLSDK.Current.Tick(31f);

            Assert.AreEqual(DataState.Loaded, JTLSDK.Data.LoadState);
            Assert.AreEqual(99, JTLSDK.Data.GetInt("Money"));
            Assert.AreEqual(2, _data.LoadCount);
        }

        [Test]
        public void CorruptedDocumentIsTreatedAsFailed()
        {
            _data.LoadResult = Providers.DataLoadResult.Loaded;
            _data.LoadPayload = "{not json";
            JTLSDK.Create(_builder.Build());

            Assert.AreEqual(DataState.Failed, JTLSDK.Data.LoadState);
        }

        [Test]
        public void SizeLimitBlocksFlush()
        {
            _data.MaxBytes = 10;
            JTLSDK.Create(_builder.Build());
            bool? flushed = null;

            JTLSDK.Data.SetString("Big", new string('x', 100));
            JTLSDK.Data.Flush(success => flushed = success);

            Assert.IsFalse(flushed);
            Assert.AreEqual(0, _data.SaveCount);
        }

        [Test]
        public void AutosaveFlushesAfterDelay()
        {
            _builder.AutosaveDelaySeconds = 0.5f;
            JTLSDK.Create(_builder.Build());

            JTLSDK.Data.SetInt("Money", 5);
            JTLSDK.Current.Tick(0.3f);
            Assert.AreEqual(0, _data.SaveCount);

            JTLSDK.Current.Tick(0.3f);
            Assert.AreEqual(1, _data.SaveCount);
            Assert.IsFalse(JTLSDK.Data.IsDirty);
        }

        [Test]
        public void WriteDuringFlushKeepsDirty()
        {
            _builder.AutosaveDelaySeconds = 0f;
            JTLSDK.Create(_builder.Build());
            JTLSDK.Data.SetInt("Money", 1);

            JTLSDK.Data.Flushed += _ => JTLSDK.Data.SetInt("Money", 2);
            JTLSDK.Data.Flush();

            Assert.IsTrue(JTLSDK.Data.IsDirty);
        }
    }
}
