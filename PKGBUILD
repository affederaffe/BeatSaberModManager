pkgname="beatsabermodmanager"
_pkgname="BeatSaberModManager"
pkgver="0.0.1b"
pkgrel="1"
arch=("x86_64")
url="https://github.com/affederaffe/BeatSaberModManager"
license=("MIT")
depends=("dotnet-runtime" "ttf-ms-fonts")
makedepends=("dotnet-sdk")
options=("!strip")
source=("$url/archive/v$pkgver.tar.gz")
sha256sums=("SKIP")

build() {
    cd "$_pkgname-$pkgver"
    dotnet publish -c Release -r linux-x64 --no-self-contained -p:PublishTrimmed=false --output ../$_pkgname
}

package() {
    install -d "$pkgdir/usr/bin"
    cp "$_pkgname/$_pkgname" "$pkgdir/usr/bin/$pkgname"
    install -Dm644 "$_pkgname/Resources/Icons/AppIcon.png" "$pkgdir/usr/share/pixmaps/$pkgname.png"
    echo "[Desktop Entry]\nName=$_pkgname\nExec=$pkgname\nIcon=$pkgname\nType=Application\nCategories=Games" > $pkgname.desktop
    install -Dm644 "$pkgname.desktop" "$pkgdir/usr/share/applications/$pkgname.desktop"
}
