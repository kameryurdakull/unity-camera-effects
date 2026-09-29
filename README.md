# Camera Framework

**Cinemachine 3 için tek servis üzerinden kamera efektleri.** Shake, recoil, FOV pulse, focus ve kamera geçişlerini oyun kodundan veya paketle gelen EventBus üzerinden çağırın.

![Cinematic Gallery demo](Distribution/com.kameryurdakull.camera-framework/Documentation~/images/gallery.png)

## Özellikler

| Efekt | Tek çağrı | Demo karşılığı |
| --- | --- | --- |
| Screen shake | `camera.Shake(CameraEffectPreset.Impact)` | Subtle, Impact, Heavy |
| Özel shake | `camera.Shake(customSettings)` | Kendi genlik ve frekansınız |
| Recoil | `camera.Recoil(new Vector3(-4, 0, 0))` | Recoil |
| FOV pulse | `camera.PulseFov(-12)` | Punch In, Pull Back |
| Focus | `await camera.FocusAsync(target)` | Focus |
| Geçiş | `await camera.SwitchAsync(nextCamera)` | Overview, Showcase, Detail |

Shake ve recoil, `CinemachineExtension` ile son kamera durumuna uygulanır. Zamanlamalar DOTween, asenkron focus/geçiş çağrıları UniTask kullanır. Efekt profili `ScriptableObject` olarak düzenlenir. `ICameraFramework` doğrudan kullanılabilir; VContainer kaydı ve EventBus yönlendirmesi ayrı assembly içindedir.

## Gereken modüller

| Modül | Sürüm / kaynak | Kullanım |
| --- | --- | --- |
| Unity | 6000.0+; 6000.3.15f1 ile doğrulandı | Paket ve demo |
| Cinemachine | `com.unity.cinemachine` 3.1.7 | Paket manifesti otomatik ister |
| UniTask | 2.5.11 | Asenkron API ve EventBus |
| VContainer | 1.19.0 | Integration assembly ve demo scope |
| DOTween | 1.3.030 | Shake, recoil, FOV ve focus animasyonları |
| ProBuilder | `com.unity.probuilder` 6.1.2 | Yalnızca demo sahnesini yeniden oluşturan editör aracı |
| URP | `com.unity.render-pipelines.universal` 17.3.0 | Demo materyallerinin görünümü |
| UI Toolkit | `com.unity.modules.uielements` 1.0.0 | Demo butonları; paket manifesti otomatik ister |

EventBus uygulaması paketin `CameraFramework.Events` modülünde bulunur. Unity MCP, paketi kullanmak için gerekli değildir. ProBuilder ve URP çekirdek runtime için gerekli değildir; **Cinematic Gallery** örneğini kullanacaksanız kurun.

### 1. Harici bağımlılıkları kurun

Unity projenizin `Packages/manifest.json` dosyasına şu Git bağımlılıklarını ekleyin:

```json
{
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.11",
    "jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.19.0"
  }
}
```

Bu satırları mevcut `dependencies` nesnenize ekleyin; dosyanın kalanını silmeyin. [DOTween 1.3.030’u resmi kaynaktan](https://dotween.demigiant.com/download) içe aktarın ve **Tools > Demigiant > DOTween Utility Panel > Setup DOTween...** adımını çalıştırın. DOTween ikili dosyaları bu repoda dağıtılmaz.

Unity, Git adreslerini **paket manifestindeki** transitif bağımlılık olarak çözmediği için UniTask ve VContainer proje manifestine ayrıca eklenir. [Unity’nin Git bağımlılığı kuralı](https://docs.unity3d.com/6000.0/Documentation/Manual/upm-git.html) bu ayrımı açıklar.

### 2. Camera Framework’ü kurun

**Window > Package Manager > + > Add package from git URL…** alanına yapıştırın:

```text
https://github.com/kameryurdakull/unity-camera-effects.git?path=/Distribution/com.kameryurdakull.camera-framework
```

Ya da `Packages/manifest.json` içindeki `dependencies` nesnesine ekleyin:

```json
"com.kameryurdakull.camera-framework": "https://github.com/kameryurdakull/unity-camera-effects.git?path=/Distribution/com.kameryurdakull.camera-framework"
```

Bu URL, repodaki Unity geliştirme projesi yerine doğrudan UPM alt klasörünü kurar. Bir sürüm etiketi yayımlandığında URL sonuna `#v1.0.0` biçiminde eklenebilir.

## Hızlı kullanım

1. Render kamerasına `CinemachineBrain`, sahneye bir veya daha fazla `CinemachineCamera` ekleyin.
2. **Assets > Create > Camera Framework > Effect Profile** ile profil oluşturun.
3. `CameraFrameworkController` ekleyip Brain, profil, kamera slotları ve başlangıç açısını bağlayın. Focus için kameraya `CinemachineRotationComposer` gibi bir Aim bileşeni ekleyin.
4. VContainer scope’unuzda servisi kaydedin:

```csharp
using CameraFramework;
using CameraFramework.Events;
using CameraFramework.Integration;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public sealed class GameScope : LifetimeScope
{
    [SerializeField] private CameraFrameworkController cameraController;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
        builder.RegisterCameraFramework(cameraController);
    }
}
```

Oyun kodunuzda `ICameraFramework` enjekte edin:

```csharp
using CameraFramework;
using UnityEngine;
using VContainer;

public sealed class WeaponCameraFeedback
{
    private readonly ICameraFramework _camera;

    [Inject]
    public WeaponCameraFeedback(ICameraFramework camera) => _camera = camera;

    public void Fire()
    {
        _camera.Recoil(new Vector3(-4f, 0f, 0f));
        _camera.Shake(CameraEffectPreset.Subtle);
        _camera.PulseFov(-8f);
    }
}
```

Event tabanlı kullanım için `IEventBus.Publish(new CameraShakeRequested(CameraEffectPreset.Impact))` çağırın. Kendi EventBus sisteminiz varsa `RegisterCameraFramework(controller, routeEvents: false)` kullanıp mevcut bus’ınızdan `ICameraFramework` çağrılarına bir adaptör yazabilirsiniz.

`CameraView` enum’u demo için hazır üç açıyı sağlar. Gerçek projelerde `SwitchAsync(CinemachineCamera camera)` aşırı yüklemesi, istediğiniz Cinemachine kameraya geçer.

## Cinematic Gallery örneği

Package Manager’da **Camera Framework > Samples > Cinematic Gallery > Import** yolunu izleyin. Örnekteki `CameraFrameworkDemo.unity` sahnesini açıp Play’e basın. Sahne; düzenlenebilir ProBuilder mimarisi, üç Cinemachine kamera, efekt profili ve UI Toolkit kontrol paneli içerir.

| Buton | Beklenen sonuç |
| --- | --- |
| Overview / Showcase / Detail | Kameralar arasında yumuşak blend |
| Subtle Shake / Impact / Heavy Shake | Artan güçte ekran sarsıntısı |
| Recoil | Kısa açısal tepki ve dönüş |
| Focus | Yandaki turkuaz işaretçiye yönelme ve geri dönüş |
| Punch In / Pull Back | Geçici görüş açısı değişimi |

**Tools > Camera Framework > Create Demo Scene** yeni bir sahne üretir; mevcut demo sahnesini değiştirmez. Örnek dosyaları [Samples~ klasöründe](Distribution/com.kameryurdakull.camera-framework/Samples~/CinematicGallery) bulunur.

## Repo düzeni

```text
Distribution/com.kameryurdakull.camera-framework/
├── package.json
├── Runtime/         # Servis arayüzü, controller, profil ve efekt extension
├── EventBus/        # Paket içi EventBus sözleşmesi ve uygulaması
├── Integration/     # VContainer kaydı ve event yönlendirmesi
├── Editor/          # Controller inspector
└── Samples~/        # İçe aktarılabilir Cinematic Gallery
```

`Assets/CameraFramework` ve Unity projesinin kalan kısmı paketi geliştirmek için kullanılan örnek projedir. UPM kurulumu yalnızca `Distribution/com.kameryurdakull.camera-framework` klasörünü alır.
