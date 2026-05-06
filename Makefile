SHELL := /usr/bin/env bash
.SHELLFLAGS := -euo pipefail -c

CLI_PROJECT := DocFormatter.Cli
SOLUTION    := DocFormatter.sln
EXAMPLES    := examples
DEFAULT_FILE := $(EXAMPLES)/1_AR_5449_2.docx
MAC_PUBLISH := $(CLI_PROJECT)/bin/Release/net10.0/osx-arm64/publish
MAC_BIN     := $(MAC_PUBLISH)/docformatter

FILE ?= $(DEFAULT_FILE)

.PHONY: help build test test-watch run run-all publish-mac publish-win clean format logs

help:
	@echo "Targets:"
	@echo "  build         dotnet build (Debug)"
	@echo "  test          dotnet test (solution)"
	@echo "  test-watch    dotnet watch test"
	@echo "  run           run CLI on FILE=<path> (default: $(DEFAULT_FILE))"
	@echo "  run-all       run CLI in batch mode on $(EXAMPLES)/"
	@echo "  publish-mac   self-contained osx-arm64 binary -> $(MAC_BIN)"
	@echo "  publish-win   delegate to $(CLI_PROJECT)/publish.sh"
	@echo "  format        dotnet format"
	@echo "  logs          tail latest formatted/_app.log under $(EXAMPLES)/"
	@echo "  clean         remove bin/, obj/, and $(EXAMPLES)/**/formatted/"

build:
	dotnet build $(SOLUTION)

test:
	dotnet test $(SOLUTION)

test-watch:
	dotnet watch --project DocFormatter.Tests test

run:
	dotnet run --project $(CLI_PROJECT) -- "$(FILE)"

run-all:
	dotnet run --project $(CLI_PROJECT) -- "$(EXAMPLES)"

publish-mac:
	dotnet publish $(CLI_PROJECT) \
		-c Release \
		-r osx-arm64 \
		--self-contained true \
		-p:PublishSingleFile=true \
		-p:IncludeNativeLibrariesForSelfExtract=true \
		-p:PublishTrimmed=false
	@echo "Artifact: $(MAC_BIN)"

publish-win:
	./$(CLI_PROJECT)/publish.sh

format:
	dotnet format $(SOLUTION)

logs:
	@latest=$$(ls -t $(EXAMPLES)/formatted/_app.log $(EXAMPLES)/**/formatted/_app.log 2>/dev/null | head -1); \
	if [[ -z "$$latest" ]]; then echo "No _app.log found under $(EXAMPLES)/"; exit 1; fi; \
	echo "==> $$latest"; tail -n 100 "$$latest"

clean:
	dotnet clean $(SOLUTION) || true
	find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
	find $(EXAMPLES) -type d -name formatted -prune -exec rm -rf {} +
