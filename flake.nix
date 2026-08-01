{
  description = "Vinland DevEnv";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { nixpkgs, flake-utils, ... }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = nixpkgs.legacyPackages.${system};
      in
      {
        devShells.default = pkgs.mkShell {
          packages = with pkgs; [
            dotnet-sdk_10
            omnisharp-roslyn
          ];

          DOTNET_ROOT = "${pkgs.dotnet-sdk_10}";
          DOTNET_ROOT_X64 = "${pkgs.dotnet-sdk_10}";
          DOTNET_CLI_TELEMETRY_OPTOUT = "1";
          DOTNET_NOLOGO = "1";
          DOTNET_MULTILEVEL_LOOKUP = "0";

          OMNISHARP_TIMEOUT = "10000";

            LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath [
            # Графика и окна
            pkgs.libGL
            pkgs.libx11          # было xorg.libX11
            pkgs.libxi           # было xorg.libXi
            pkgs.libxcursor      # было xorg.libXcursor
            pkgs.libxrandr       # было xorg.libXrandr
            pkgs.libxinerama     # было xorg.libXinerama
            
            # Wayland (на случай, если MonoGame или SDL решат его использовать)
            pkgs.wayland
            pkgs.libxkbcommon
            
            # Звук
            pkgs.alsa-lib
            pkgs.openal
            
            # Шрифты
            pkgs.fontconfig
            pkgs.freetype
          ];

          # SSL-сертификаты (нужно для NuGet restore)
          SSL_CERT_FILE = "${pkgs.cacert}/etc/ssl/certs/ca-bundle.crt";
        };
      }
    );
}