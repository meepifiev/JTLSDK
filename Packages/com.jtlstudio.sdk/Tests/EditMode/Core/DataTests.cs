using System;
using JTLStudio.SDK.Providers;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests
{
    public class DataTests
    {
        [Serializable]
        public class Profile
        {
            public string Name = "Player";
            public int Level = 1;
            public float Volume = 0.5f;
        }

        private TestSettings _settings;
        private FakeDataProvider _data;

        [SetUp]
        public void SetUp()
        {
            _data = new FakeDataProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _settings?.Dispose();
            _settings = null;
        }

        [Test]
        public void EmptyStorageIsReadyAndWritable()
        {
            Create();

            Assert.That(JTLSDK.Data.LoadState, Is.EqualTo(DataState.Empty));
            Assert.That(JTLSDK.Data.IsReady, Is.True);

            JTLSDK.Data.SetInt("Money", 1200);
            JTLSDK.Data.SetFloat("Volume", 0.5f);
            JTLSDK.Data.SetBool("Tutorial", true);
            JTLSDK.Data.SetString("Name", "Player");

            Assert.That(JTLSDK.Data.IsDirty, Is.True);
            Assert.That(JTLSDK.Data.GetInt("Money"), Is.EqualTo(1200));
            Assert.That(JTLSDK.Data.GetFloat("Volume"), Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(JTLSDK.Data.GetBool("Tutorial"), Is.True);
            Assert.That(JTLSDK.Data.GetString("Name"), Is.EqualTo("Player"));
        }

        [Test]
        public void FlushWritesJsonAndReloadRestoresValues()
        {
            Create();
            JTLSDK.Data.SetInt("Money", 1200);
            JTLSDK.Data.SetObject("Profile", new Profile { Name = "Ann", Level = 27, Volume = 0.25f });

            bool flushed = false;
            JTLSDK.Data.Flush(success => flushed = success);

            Assert.That(flushed, Is.True);
            Assert.That(JTLSDK.Data.IsDirty, Is.False);
            Assert.That(_data.Stored, Does.Contain("\"Money\":1200"));
            Assert.That(_data.Stored, Does.Contain("\"revision\":1"));

            _settings.Dispose();
            _settings = null;
            _data.LoadResult = DataLoadResult.Loaded;
            Create();

            Assert.That(JTLSDK.Data.LoadState, Is.EqualTo(DataState.Loaded));
            Assert.That(JTLSDK.Data.GetInt("Money"), Is.EqualTo(1200));

            Profile profile = JTLSDK.Data.GetObject<Profile>("Profile");

            Assert.That(profile.Name, Is.EqualTo("Ann"));
            Assert.That(profile.Level, Is.EqualTo(27));
            Assert.That(profile.Volume, Is.EqualTo(0.25f).Within(0.0001f));
        }

        [Test]
        public void ObjectFieldsMissingInSaveKeepDefaults()
        {
            _data.LoadResult = DataLoadResult.Loaded;
            _data.Stored = "{\"format\":1,\"revision\":3,\"values\":{\"Profile\":{\"Level\":9}}}";
            Create();

            Profile profile = JTLSDK.Data.GetObject<Profile>("Profile");

            Assert.That(profile.Level, Is.EqualTo(9));
            Assert.That(profile.Name, Is.EqualTo("Player"));
            Assert.That(profile.Volume, Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void FailedLoadBlocksWritesToStorage()
        {
            _data.LoadResult = DataLoadResult.Failed;
            Create();

            Assert.That(JTLSDK.Data.LoadState, Is.EqualTo(DataState.Failed));
            Assert.That(JTLSDK.Data.State, Is.EqualTo(ModuleState.Failed));

            JTLSDK.Data.SetInt("Money", 5);
            bool flushed = true;
            JTLSDK.Data.Flush(success => flushed = success);

            Assert.That(JTLSDK.Data.GetInt("Money"), Is.EqualTo(5));
            Assert.That(flushed, Is.False);
            Assert.That(_data.SaveCalls, Is.EqualTo(0));
        }

        [Test]
        public void CorruptedSaveIsTreatedAsFailure()
        {
            _data.LoadResult = DataLoadResult.Loaded;
            _data.Stored = "{not json";
            Create();

            Assert.That(JTLSDK.Data.LoadState, Is.EqualTo(DataState.Failed));
            Assert.That(_data.SaveCalls, Is.EqualTo(0));
        }

        [Test]
        public void AutosaveFlushesAfterDelay()
        {
            _settings = new TestSettings().WithData(_data).WithAutosaveDelay(2f);
            _settings.Create();
            JTLSDK.Data.SetInt("Money", 1);

            JTLSDK.Current.Tick(1f);

            Assert.That(_data.SaveCalls, Is.EqualTo(0));

            JTLSDK.Current.Tick(1.5f);

            Assert.That(_data.SaveCalls, Is.EqualTo(1));
            Assert.That(JTLSDK.Data.IsDirty, Is.False);
        }

        [Test]
        public void OversizedSaveIsRejected()
        {
            _data.MaxBytes = 64;
            Create();
            JTLSDK.Data.SetString("Blob", new string('x', 200));

            bool flushed = true;
            JTLSDK.Data.Flush(success => flushed = success);

            Assert.That(flushed, Is.False);
            Assert.That(_data.SaveCalls, Is.EqualTo(0));
        }

        private void Create()
        {
            _settings = new TestSettings().WithData(_data);
            _settings.Create();
        }
    }
}
