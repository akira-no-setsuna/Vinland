{
  description = "Vinland MonoGame DevEnv";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { nixpkgs, flake-utils, ... }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = nixpkgs.legacyPackages.${system};
        runtimeLibs = with pkgs; [

          stdenv.cc.cc.lib   # libc, libstdc++
          zlib               # сжатие
          icu                # интернационализация
          openssl            # HTTPS

          # MonoGame
          SDL2               # окно, ввод, рендеринг
          openal             # звук (OpenAL Soft)
          libGL              # OpenGL
          libglvnd
          libudev-zero       # геймпады / hotplug

          libvorbis          # .ogg аудио
          libogg
          freetype           # шрифты (SpriteFont)

          libX11
          libXcursor
          libXi
          libXrandr
          libXext
        ];
      in
      {
        devShells.default = pkgs.mkShell {
          packages = with pkgs; [
            dotnet-sdk_10
            omnisharp-roslyn
            dotnet-mgcb  # MonoGame Content Builder
          ];

          DOTNET_ROOT = "${pkgs.dotnet-sdk_10}";
          DOTNET_ROOT_X64 = "${pkgs.dotnet-sdk_10}";
          DOTNET_CLI_TELEMETRY_OPTOUT = "1";
          DOTNET_NOLOGO = "1";
          DOTNET_MULTILEVEL_LOOKUP = "0";

          OMNISHARP_TIMEOUT = "10000";

          NIX_LD = pkgs.lib.fileContents "${pkgs.stdenv.cc}/nix-support/dynamic-linker";
          NIX_LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath runtimeLibs;

          LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath runtimeLibs;

          SSL_CERT_FILE = "${pkgs.cacert}/etc/ssl/certs/ca-bundle.crt";

          __NV_PRIME_RENDER_OFFLOAD = "1";
          __GLX_VENDOR_LIBRARY_NAME = "nvidia";
          __VK_LAYER_NV_optimus = "NVIDIA_only";
        };
      }
    );
}