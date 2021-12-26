pkgname="beatsabermodmanager"
_pkgname="BeatSaberModManager"
pkgver="0.0.1b"
pkgrel="1"
pkgdesc="Yet another mod installer for Beat Saber, heavily inspired by ModAssistant"
arch=("x86_64")
url="https://github.com/affederaffe/BeatSaberModManager"
license=("MIT")
depends=("dotnet-runtime")
makedepends=("dotnet-sdk" "git" "imagemagick" "gendesk")
options=("!strip")
source=("$url/archive/v$pkgver.tar.gz")
sha256sums=("SKIP")

build() {
    cd "$_pkgname-$pkgver"
    git clone https://github.com/geefr/BSIPA-Linux.git
    dotnet publish -c Release -r linux-x64 --no-self-contained -p:PublishTrimmed=false --output ../$_pkgname
}

package() {
    convert "$_pkgname/Resources/Icons/Icon.ico" "$pkgname.png"
    gendesk -n --pkgname "$pkgname" --name "$_pkgname" --pkgdesc "$pkgdesc" --comment "$pkgdesc" --categories "Game;Utility" --icon "$pkgname.png"
    install -Dm755 "$_pkgname/$_pkgname" "$pkgdir/usr/bin/$pkgname"
    install -Dm644 "$pkgname.png" "$pkgdir/usr/share/pixmaps/$pkgname.png"
    install -Dm644 "$pkgname.desktop" "$pkgdir/usr/share/applications/$pkgname.desktop"
}