import { utils } from "@xxyy/vue-app";

export class AccessVerifyHelper {
  public HEADER_NAME = "aiuo-sign";
  private _bothKeySeed = "hNMmcYykGdCluYqe"; //与服务器同步
  private _bothKeyIndexes = [
    7, 1, 4, 15, 5, 2, 0, 8, 13, 14, 9, 12, 11, 10, 6, 3,
  ]; //与服务器同步

  /** 使用bothkey获取签名 */
  public signBothKey(sourceKey: string, sourceData: string) {
    var bothKey = this.getBothKey(sourceKey);
    return utils.hmacSHA256(sourceData, bothKey);
  }

  /** 解密返回的AccessKey */
  public decryptAccessKey(sourceKey: string, accessKey: string) {
    var bothKey = this.getBothKey(sourceKey);
    return utils.aesDecrypt(accessKey, bothKey);
  }

  /** 使用AccessKey获取签名 */
  public signAccessKey(
    sourceKey: string,
    accessKey: string,
    sourceData: string
  ) {
    var sign = utils.hmacSHA256(sourceData, accessKey);
    return `${sourceKey}|${sign}`;
  }

  private getBothKey(sourceKey: string) {
    return this.getKey(this._bothKeySeed, this._bothKeyIndexes, sourceKey);
  }
  private getKey(constSeed: string, constIndexes: number[], sourceKey: string) {
    var len = constIndexes.length;
    var mod = sourceKey.length % len;
    sourceKey += constSeed.substring(0, len - mod);
    var max = sourceKey.length / len;
    var ret = "";
    for (var i = 0; i < constIndexes.length; i++) {
      var idx = (i % max) * len;
      ret += sourceKey[idx + constIndexes[i]];
    }
    return ret;
  }
}
