/*
 * Copyright (C) 2022 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License
 */
package com.android.tools.build.bundletool.sdkmodule;

import static com.google.common.collect.ImmutableList.toImmutableList;

import com.android.bundle.SdkModulesConfigOuterClass.SdkModulesConfig;
import com.android.tools.build.bundletool.model.BundleModule;
import com.android.tools.build.bundletool.model.ModuleEntriesMutator;
import com.android.tools.build.bundletool.model.ModuleEntry;
import com.android.tools.build.bundletool.model.ZipPath;
import com.google.common.collect.ImmutableList;
import java.util.function.Function;
import java.util.function.Predicate;

/**
 * Mutator that moves Java resources of a given SDK module to the assets directory, so that SDK
 * classes are not loaded together with the app.
 */
public class JavaResourceRepackager extends ModuleEntriesMutator {

  private static final String ASSETS_DIRECTORY = "assets";
  private static final String ASSETS_SUBDIRECTORY_PREFIX = "RuntimeEnabledSdk-";

  private final SdkModulesConfig sdkModulesConfig;

  JavaResourceRepackager(SdkModulesConfig sdkModulesConfig) {
    this.sdkModulesConfig = sdkModulesConfig;
  }

  @Override
  public Predicate<ModuleEntry> getFilter() {
    return entry -> entry.getPath().toString().startsWith("root/");
  }

  @Override
  public Function<ImmutableList<ModuleEntry>, ImmutableList<ModuleEntry>> getMutator() {
    return (resourceEntries) ->
        resourceEntries.stream().map(this::updateJavaResourceEntryPath).collect(toImmutableList());
  }

  @Override
  public boolean shouldApplyMutation(BundleModule module) {
    return true;
  }

  private ModuleEntry updateJavaResourceEntryPath(ModuleEntry javaResourceEntry) {
    ZipPath javaResourceEntryPath = javaResourceEntry.getPath();
    if (javaResourceEntryPath.getNameCount() < 2) {
      throw new IllegalStateException(
          "Unexpected path to a Java resource entry: " + javaResourceEntryPath);
    }
    String javaResourceFilePath =
        javaResourceEntryPath.subpath(1, javaResourceEntryPath.getNameCount()).toString();
    return javaResourceEntry.toBuilder()
        .setPath(ZipPath.create(getNewJavaResourceDirectoryPath() + "/" + javaResourceFilePath))
        .build();
  }

  String getNewJavaResourceDirectoryPath() {
    return ASSETS_DIRECTORY + "/" + getNewJavaResourceDirectoryPathInsideAssets();
  }

  String getNewJavaResourceDirectoryPathInsideAssets() {
    return ASSETS_SUBDIRECTORY_PREFIX + sdkModulesConfig.getSdkPackageName() + "/javaresources";
  }
}
